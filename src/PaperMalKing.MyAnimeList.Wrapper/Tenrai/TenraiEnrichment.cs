// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Every failure is projected onto an outcome")]
internal sealed class TenraiEnrichment : IMyAnimeListEnrichment
{
	private const string AnimeOperation = "anime";
	private const string MangaOperation = "manga";
	private const string SeiyuOperation = "characters";
	private const string CharacterOperation = "character";
	private const string PersonOperation = "person";
	private const string PersonVoicesOperation = "person voices";
	private const string StudioOperation = "studio";
	private const string MainRole = "Main";

	private readonly TenraiClient _client;
	private readonly TenraiGate _gate;
	private readonly ILogger<TenraiEnrichment> _logger;

	public TenraiEnrichment(ILogger<TenraiEnrichment> logger, HttpClient httpClient, TenraiGate gate)
	{
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(httpClient);
		ArgumentNullException.ThrowIfNull(gate);
		this._logger = logger;
		this._client = new(httpClient);
		this._gate = gate;
	}

	public Task<MediaInfo> GetAnimeDetailsAsync(long id, CancellationToken cancellationToken)
	{
		this._logger.RequestingAnimeDetails(id);
		return this.RunAsync(AnimeOperation, id, MediaInfo.Empty, this.GetAnimeDetailsOutcomeAsync, cancellationToken);
	}

	public Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken)
	{
		this._logger.RequestingMangaDetails(id);
		return this.RunAsync(MangaOperation, id, MediaInfo.Empty, this.GetMangaDetailsOutcomeAsync, cancellationToken);
	}

	public Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken)
	{
		this._logger.RequestingSeiyuDetails(id);
		return this.RunAsync<IReadOnlyList<SeyuInfo>>(SeiyuOperation, id, [], this.GetAnimeSeiyuOutcomeAsync, cancellationToken);
	}

	public Task<EntityInfo> GetCharacterInfoAsync(long id, bool withDescription, CancellationToken cancellationToken)
	{
		this._logger.RequestingCharacterInfo(id);
		return withDescription
			? this.RunAsync(CharacterOperation, id, EntityInfo.Empty, this.GetCharacterFullOutcomeAsync, cancellationToken)
			: this.RunAsync(CharacterOperation, id, EntityInfo.Empty, this.GetCharacterAnimeOutcomeAsync, cancellationToken);
	}

	public async Task<EntityInfo> GetPersonInfoAsync(long id, bool withDescription, CancellationToken cancellationToken)
	{
		this._logger.RequestingPersonInfo(id);
		var voices = await this.RunAsync(PersonVoicesOperation, id, EntityInfo.Empty, this.GetPersonVoicesOutcomeAsync, cancellationToken);
		if (!withDescription)
		{
			return voices;
		}

		var about = await this.RunAsync(PersonOperation, id, EntityInfo.Empty, this.GetPersonDescriptionOutcomeAsync, cancellationToken);
		return new() { Description = about.Description, BestKnownWork = voices.BestKnownWork, };
	}

	public Task<EntityInfo> GetStudioInfoAsync(long id, CancellationToken cancellationToken)
	{
		this._logger.RequestingStudioInfo(id);
		return this.RunAsync(StudioOperation, id, EntityInfo.Empty, this.GetStudioOutcomeAsync, cancellationToken);
	}

	internal Task<TenraiEnrichmentOutcome<MediaInfo>> GetAnimeDetailsOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetAnimeByIdAsync, ProjectMedia, cancellationToken);

	internal Task<TenraiEnrichmentOutcome<MediaInfo>> GetMangaDetailsOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetMangaByIdAsync, ProjectMedia, cancellationToken);

	internal Task<TenraiEnrichmentOutcome<IReadOnlyList<SeyuInfo>>> GetAnimeSeiyuOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetAnimeByIdCharactersAsync, ProjectSeiyu, cancellationToken);

	internal Task<TenraiEnrichmentOutcome<EntityInfo>> GetCharacterFullOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetCharactersByIdFullAsync, ProjectCharacterFull, cancellationToken);

	internal Task<TenraiEnrichmentOutcome<EntityInfo>> GetCharacterAnimeOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetCharactersByIdAnimeAsync, ProjectCharacterAnime, cancellationToken);

	internal Task<TenraiEnrichmentOutcome<EntityInfo>> GetPersonVoicesOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetPeopleByIdVoicesAsync, ProjectPersonVoices, cancellationToken);

	internal Task<TenraiEnrichmentOutcome<EntityInfo>> GetPersonDescriptionOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetPeopleByIdAsync, ProjectPersonDescription, cancellationToken);

	internal Task<TenraiEnrichmentOutcome<EntityInfo>> GetStudioOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptCoreAsync(
			token => this._client.GetAnimeAsync(id.ToString(CultureInfo.InvariantCulture), Order_by.Members, Sort.Desc, limit: 1, token),
			ProjectStudio,
			cancellationToken);

	private static MediaInfo? ProjectMedia(MediaResponse response)
	{
		var data = response.Data;
		if (data.Themes is null && data.Demographics is null)
		{
			return null;
		}

		var themes = ValidNames(data.Themes);
		var demographic = ValidNames(data.Demographics);
		return themes.Length is 0 && demographic.Length is 0 ? MediaInfo.Empty : new() { Themes = themes, Demographic = demographic, };
	}

	private static IReadOnlyList<SeyuInfo>? ProjectSeiyu(CharactersResponse response)
	{
		ICollection<Character>? characters = response.Data;
		if (characters is null)
		{
			return null;
		}

		var result = new List<SeyuInfo>();
		foreach (var character in characters)
		{
			AddValidSeiyu(character?.Voice_actors, result);
		}

		return result;
	}

	private static EntityInfo? ProjectCharacterFull(CharacterFullResponse response)
	{
		var data = response.Data;
		return data.About is null && data.Anime is null
			? null
			: new EntityInfo { Description = Trimmed(data.About), BestKnownWork = BestKnownWorkOf(data.Anime), };
	}

	private static EntityInfo? ProjectCharacterAnime(CharacterAnimeResponse response) =>
		response.Data is null ? null : new EntityInfo { BestKnownWork = BestKnownWorkOf(response.Data), };

	private static EntityInfo? ProjectPersonVoices(PersonVoicesResponse response) =>
		response.Data is null ? null : new EntityInfo { BestKnownWork = BestKnownWorkOf(response.Data), };

	private static EntityInfo? ProjectPersonDescription(PersonResponse response) =>
		response.Data.About is null ? null : new EntityInfo { Description = Trimmed(response.Data.About), };

	private static EntityInfo? ProjectStudio(AnimeSearchResponse response) =>
		response.Data is null
			? null
			: new EntityInfo { BestKnownWork = FirstWork(response.Data.Select(static entry => ToBestKnownWork(entry))), };

	private static BestKnownWork? BestKnownWorkOf(IEnumerable<CharacterAnimeEntry>? entries) =>
		entries is null
			? null
			: FirstWork(entries.Where(static entry => string.Equals(entry.Role, MainRole, StringComparison.OrdinalIgnoreCase))
							   .Select(static entry => ToBestKnownWork(entry.Anime)));

	private static BestKnownWork? BestKnownWorkOf(IEnumerable<PersonVoiceEntry>? entries) =>
		entries is null
			? null
			: FirstWork(entries.Where(static entry => string.Equals(entry.Role, MainRole, StringComparison.OrdinalIgnoreCase))
							   .Select(static entry => ToBestKnownWork(entry.Anime)));

	private static BestKnownWork? FirstWork(IEnumerable<BestKnownWork?> works) => works.FirstOrDefault(static work => work is not null);

	private static BestKnownWork? ToBestKnownWork(AnimeReference? anime) =>
		anime is not null && !string.IsNullOrWhiteSpace(anime.Title) && IsMyAnimeListUrl(anime.Url)
			? new BestKnownWork { Title = anime.Title.Trim(), Url = anime.Url, }
			: null;

	private static string? Trimmed(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

	private static string[] ValidNames(IEnumerable<CatalogReference>? entries) => entries?
		.Where(static entry => !string.IsNullOrWhiteSpace(entry.Name))
		.Select(static entry => entry.Name!)
		.ToArray() ?? [];

	private static long ElapsedMilliseconds(long start) => (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds;

	private static TenraiEnrichmentOutcome<TValue> Failure<TValue>(Exception exception, long start) =>
		(TenraiClassification.Fault(exception), exception) switch
		{
			(TenraiFault.Suppressed, TenraiSuppressedException suppressed) => new TenraiEnrichmentOutcome<TValue>.Suppressed(suppressed.Reason),
			(TenraiFault.Api, TenraiApiException api) => ApiFailure<TValue>(api, start),
			(TenraiFault.Transport, TenraiTransportException transport) => new TenraiEnrichmentOutcome<TValue>.Failed(
				TenraiFailureKind.Transport, Status: null, transport.Facts, ElapsedMilliseconds(start)),
			_ => new TenraiEnrichmentOutcome<TValue>.Failed(TenraiFailureKind.Transport, Status: null, default, ElapsedMilliseconds(start)),
		};

	private static TenraiEnrichmentOutcome<TValue> ApiFailure<TValue>(TenraiApiException exception, long start)
	{
		var disposition = TenraiClassification.Classify(exception.StatusCode);
		return disposition is TenraiDisposition.NotFound
			? new TenraiEnrichmentOutcome<TValue>.NotFound()
			: new TenraiEnrichmentOutcome<TValue>.Failed(TenraiClassification.FailureKind(disposition), exception.StatusCode,
				TenraiAttempt.Read(exception.Headers), ElapsedMilliseconds(start));
	}

	private static void AddValidSeiyu(IEnumerable<VoiceActor>? actors, List<SeyuInfo> result)
	{
		if (actors is null)
		{
			return;
		}

		foreach (var actor in actors)
		{
			var person = actor?.Person;
			if (!string.Equals(actor?.Language, "Japanese", StringComparison.Ordinal) ||
				string.IsNullOrWhiteSpace(person?.Name) ||
				!IsMyAnimeListUrl(person.Url))
			{
				continue;
			}

			result.Add(new() { Name = person.Name, Url = person.Url, });
		}
	}

	private static bool IsMyAnimeListUrl([NotNullWhen(true)] string? url)
	{
		if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
			(!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
			 !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
		{
			return false;
		}

		return uri.Host.Equals("myanimelist.net", StringComparison.OrdinalIgnoreCase) ||
			uri.Host.EndsWith(".myanimelist.net", StringComparison.OrdinalIgnoreCase);
	}

	private Task<TenraiEnrichmentOutcome<TValue>> AttemptAsync<TPayload, TValue>(
		long id,
		Func<int, CancellationToken, Task<TenraiResponse<TPayload>>> request,
		Func<TPayload, TValue?> project,
		CancellationToken cancellationToken)
		where TValue : class =>
		this.AttemptCoreAsync(token => request(checked((int)id), token), project, cancellationToken);

	private async Task<TenraiEnrichmentOutcome<TValue>> AttemptCoreAsync<TPayload, TValue>(
		Func<CancellationToken, Task<TenraiResponse<TPayload>>> request,
		Func<TPayload, TValue?> project,
		CancellationToken cancellationToken)
		where TValue : class
	{
		if (this._gate.Check() is { } suppression)
		{
			return new TenraiEnrichmentOutcome<TValue>.Suppressed(suppression);
		}

		var start = Stopwatch.GetTimestamp();
		try
		{
			var response = await request(cancellationToken);
			return project(response.Result) is { } value
				? new TenraiEnrichmentOutcome<TValue>.Enriched(value)
				: new TenraiEnrichmentOutcome<TValue>.Failed(
					TenraiClassification.FailureKind(TenraiDisposition.Success), Status: null, TenraiAttempt.Read(response.Headers),
					ElapsedMilliseconds(start));
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			return Failure<TValue>(exception, start);
		}
	}

	private async Task<TValue> RunAsync<TValue>(
		string operation,
		long id,
		TValue fallback,
		Func<long, CancellationToken, Task<TenraiEnrichmentOutcome<TValue>>> attempt,
		CancellationToken cancellationToken)
	{
		var outcome = await attempt(id, cancellationToken);
		TenraiEnrichmentReport.Report(this._logger, operation, id, outcome);
		if (outcome is TenraiEnrichmentOutcome<TValue>.Failed failed && TenraiClassification.OpensCircuit(failed.Kind))
		{
			_ = this._gate.Record(TenraiSignal.Failed);
		}

		return outcome is TenraiEnrichmentOutcome<TValue>.Enriched enriched ? enriched.Value : fallback;
	}
}
