// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class MyAnimeListClientSearchTests
{
	[Test]
	public async Task AnimeSearchRequestsTheCompleteSnapshotAndDeserializesIt()
	{
		const uint expectedId = 5114U;
		const uint expectedEpisodes = 64U;
		const double expectedMean = 9.1;
		const uint expectedYear = 2009U;
		const uint expectedMembers = 3_500_000U;
		var expectedStartDate = new DateOnly(2009, 4, 5);
		HttpRequestMessage? capturedRequest = null;
		using var handler = new FakeHttpMessageHandler(request =>
		{
			capturedRequest = request;
			return JsonResponse(
				"{\"data\":[{\"node\":{\"id\":5114,\"title\":\"Fullmetal Alchemist: Brotherhood\"," +
				"\"main_picture\":{\"medium\":\"https://example.test/medium.jpg\",\"large\":\"https://example.test/large.jpg\"}," +
				"\"alternative_titles\":{\"synonyms\":[],\"en\":\"\",\"ja\":null},\"media_type\":\"future_anime_type\"," +
				"\"status\":\"finished_airing\",\"num_episodes\":64,\"mean\":9.1,\"start_date\":\"2009-04-05\"," +
				"\"start_season\":{\"year\":2009,\"season\":\"spring\"},\"num_list_users\":3500000," +
				"\"genres\":[{\"name\":\"Action\"}],\"synopsis\":\"Two brothers search for a Philosopher's Stone.\"," +
				"\"nsfw\":\"white\"}}]}");
		});
		using var scope = new ClientScope(handler);
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		var response = await scope.Client.SearchAnimeAsync("Fate/Zero & 日本", includeNsfw: true, cancellationToken);

		await Assert.That(capturedRequest).IsNotNull();
		var uri = capturedRequest!.RequestUri!;
		await Assert.That(uri.AbsolutePath).IsEqualTo("/v2/anime");
		await Assert.That(uri.Query).Contains("q=Fate%2FZero%20%26%20%E6%97%A5%E6%9C%AC");
		await Assert.That(uri.Query).Contains("limit=100");
		await Assert.That(uri.Query).Contains("offset=0");
		await Assert.That(uri.Query).Contains("nsfw=true");
		await Assert.That(GetFields(uri)).IsEquivalentTo([
			"id",
			"title",
			"main_picture",
			"alternative_titles",
			"media_type",
			"status",
			"num_episodes",
			"mean",
			"start_date",
			"start_season",
			"num_list_users",
			"genres{name}",
			"synopsis",
			"nsfw",
		]);

		var result = response.Results.Single().Result;
		await Assert.That(result.Id).IsEqualTo(expectedId);
		await Assert.That(result.MediaType).IsEqualTo(AnimeMediaType.Unknown);
		await Assert.That(result.Episodes).IsEqualTo(expectedEpisodes);
		await Assert.That(result.Mean).IsEqualTo(expectedMean);
		await Assert.That(result.StartDate).IsEqualTo(expectedStartDate);
		await Assert.That(result.StartSeason).IsNotNull();
		await Assert.That(result.StartSeason.Year).IsEqualTo(expectedYear);
		await Assert.That(result.StartSeason.Season).IsEqualTo(AnimeSeason.Spring);
		await Assert.That(result.AlternativeTitles).IsNotNull();
		await Assert.That(result.AlternativeTitles.Synonyms).IsEmpty();
		await Assert.That(result.AlternativeTitles.English).IsEmpty();
		await Assert.That(result.AlternativeTitles.Japanese).IsNull();
		await Assert.That(result.ListUserCount).IsEqualTo(expectedMembers);
		await Assert.That(result.Genres).IsNotNull();
		await Assert.That(result.Genres.Single().Name).IsEqualTo("Action");
		await Assert.That(result.Synopsis).IsEqualTo("Two brothers search for a Philosopher's Stone.");
		await Assert.That(result.Nsfw).IsEqualTo(NsfwCategory.White);
	}

	[Test]
	public async Task MangaSearchOmitsNsfwAndToleratesMissingOptionalProperties()
	{
		const uint firstId = 2U;
		const uint secondId = 3U;
		const uint expectedChapters = 380U;
		const uint expectedVolumes = 43U;
		var expectedStartDate = new DateOnly(1989, 8, 25);
		HttpRequestMessage? capturedRequest = null;
		using var handler = new FakeHttpMessageHandler(request =>
		{
			capturedRequest = request;
			return JsonResponse(
				"{\"data\":[{\"node\":{\"id\":2,\"title\":\"Berserk\",\"alternative_titles\":{}," +
				"\"media_type\":\"future_manga_type\",\"status\":\"currently_publishing\",\"num_chapters\":380," +
				"\"num_volumes\":43,\"start_date\":\"1989-08-25\",\"num_list_users\":700000}},{\"node\":{\"id\":3," +
				"\"title\":\"Vagabond\",\"media_type\":\"manga\",\"status\":\"on_hiatus\",\"num_chapters\":327," +
				"\"num_volumes\":37,\"num_list_users\":400000,\"genres\":null}}]}");
		});
		using var scope = new ClientScope(handler);
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		var response = await scope.Client.SearchMangaAsync("Blue Lock", includeNsfw: false, cancellationToken);

		await Assert.That(capturedRequest).IsNotNull();
		var uri = capturedRequest!.RequestUri!;
		await Assert.That(uri.AbsolutePath).IsEqualTo("/v2/manga");
		await Assert.That(uri.Query).Contains("q=Blue%20Lock");
		await Assert.That(uri.Query).Contains("limit=100");
		await Assert.That(uri.Query).Contains("offset=0");
		await Assert.That(uri.Query).DoesNotContain("nsfw=");
		await Assert.That(GetFields(uri)).IsEquivalentTo([
			"id",
			"title",
			"main_picture",
			"alternative_titles",
			"media_type",
			"status",
			"num_chapters",
			"num_volumes",
			"mean",
			"start_date",
			"num_list_users",
			"genres{name}",
			"synopsis",
			"nsfw",
		]);

		var first = response.Results.Single(envelope => envelope.Result.Id == firstId).Result;
		await Assert.That(first.MediaType).IsEqualTo(MangaMediaType.Unknown);
		await Assert.That(first.Chapters).IsEqualTo(expectedChapters);
		await Assert.That(first.Volumes).IsEqualTo(expectedVolumes);
		await Assert.That(first.StartDate).IsEqualTo(expectedStartDate);
		await Assert.That(first.Picture).IsNull();
		await Assert.That(first.Mean).IsNull();
		await Assert.That(first.Synopsis).IsNull();
		await Assert.That(first.Nsfw).IsNull();
		await Assert.That(first.AlternativeTitles).IsNotNull();
		await Assert.That(first.AlternativeTitles.Synonyms).IsNull();
		await Assert.That(first.AlternativeTitles.English).IsNull();
		await Assert.That(first.AlternativeTitles.Japanese).IsNull();
		await Assert.That(first.Genres).IsNull();
		var second = response.Results.Single(envelope => envelope.Result.Id == secondId).Result;
		await Assert.That(second.AlternativeTitles).IsNull();
		await Assert.That(second.Genres).IsNull();
	}

	[Test]
	[Arguments("anime")]
	[Arguments("manga")]
	public async Task SearchPreservesNonSuccessStatus(string mediaPath)
	{
		using var handler = new FakeHttpMessageHandler(_ => new(HttpStatusCode.Forbidden));
		using var scope = new ClientScope(handler);
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		HttpStatusCode? statusCode = null;

		try
		{
			if (mediaPath.Equals("anime", StringComparison.Ordinal))
			{
				_ = await scope.Client.SearchAnimeAsync("Monster", includeNsfw: false, cancellationToken);
			}
			else
			{
				_ = await scope.Client.SearchMangaAsync("Monster", includeNsfw: false, cancellationToken);
			}
		}
		catch (HttpRequestException ex)
		{
			statusCode = ex.StatusCode;
		}

		await Assert.That(statusCode).IsEqualTo(HttpStatusCode.Forbidden);
	}

	private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
	{
		Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
	};

	private static string[] GetFields(Uri uri)
	{
		var fieldsPart = uri.Query[1..].Split('&', StringSplitOptions.RemoveEmptyEntries)
			.Single(static part => part.StartsWith("fields=", StringComparison.Ordinal));
		return Uri.UnescapeDataString(fieldsPart["fields=".Length..]).Split(',');
	}

	private sealed class ClientScope : IDisposable
	{
		private readonly FakeHttpMessageHandler _unofficialHandler;
		private readonly HttpClient _unofficialClient;
		private readonly HttpClient _officialClient;

		public ClientScope(HttpMessageHandler officialHandler)
		{
			this._unofficialHandler = new(_ => throw new InvalidOperationException("The unofficial client should not be used"));
			this._unofficialClient = new(this._unofficialHandler, disposeHandler: false);
			this._officialClient = new(officialHandler, disposeHandler: false);
			this.Client = new(NullLogger<MyAnimeListClient>.Instance, this._unofficialClient, this._officialClient, null!);
		}

		public MyAnimeListClient Client { get; }

		public void Dispose()
		{
			this._unofficialClient.Dispose();
			this._officialClient.Dispose();
			this._unofficialHandler.Dispose();
		}
	}

	private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			Task.FromResult(respond(request));
	}
}
