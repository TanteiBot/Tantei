// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

	internal Task<TenraiEnrichmentOutcome<MediaInfo>> GetAnimeDetailsOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetAnimeByIdAsync, ProjectMedia, cancellationToken);

	internal Task<TenraiEnrichmentOutcome<MediaInfo>> GetMangaDetailsOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetMangaByIdAsync, ProjectMedia, cancellationToken);

	internal Task<TenraiEnrichmentOutcome<IReadOnlyList<SeyuInfo>>> GetAnimeSeiyuOutcomeAsync(long id, CancellationToken cancellationToken) =>
		this.AttemptAsync(id, this._client.GetAnimeByIdCharactersAsync, ProjectSeiyu, cancellationToken);

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
				!IsMyAnimeListPersonUrl(person.Url))
			{
				continue;
			}

			result.Add(new() { Name = person.Name, Url = person.Url, });
		}
	}

	private static bool IsMyAnimeListPersonUrl([NotNullWhen(true)] string? url)
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

	private async Task<TenraiEnrichmentOutcome<TValue>> AttemptAsync<TPayload, TValue>(
		long id,
		Func<int, CancellationToken, Task<TenraiResponse<TPayload>>> request,
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
			var response = await request(checked((int)id), cancellationToken);
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
