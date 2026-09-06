// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using GraphQL.Client.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.AniList.UpdateProvider.Search;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;
using PaperMalKing.UpdatesProviders.Base.Search;
using TUnit.Assertions.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Tests.Search;

public sealed class AniListMediaSearchServiceTests
{
	private const string Query = "Monster";
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task SearchAnimeAutoPostsAnExactPrimaryMatchAndDeletesTheEphemeral()
	{
		var client = new FakeAniListSearchClient { Response = Response(Media(id: 1U, romaji: Query)) };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, format: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsEqualTo(1);
		await Assert.That(client.Types.Single()).IsEqualTo(ListType.Anime);
		await Assert.That(client.UserIds.Single()).IsNull();
		await Assert.That(target.Operations).IsEquivalentTo(
			[FakeSearchMessageTarget.PostOperation, FakeSearchMessageTarget.DeleteOperation],
			CollectionOrdering.Matching);
		await Assert.That(target.Posts.Single().Title).StartsWith(Query);
	}

	[Test]
	public async Task SearchMangaQueriesTheClientWithTheMangaType()
	{
		var client = new FakeAniListSearchClient { Response = Response(Media(id: 1U, romaji: Query)) };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchMangaAsync(new FakeSearchInvocation(target), Query, format: null, CancellationToken.None);

		await Assert.That(client.Types.Single()).IsEqualTo(ListType.Manga);
	}

	[Test]
	public async Task SearchAppliesTheSelectedFormatToTheClient()
	{
		var client = new FakeAniListSearchClient { Response = Response(Media(id: 1U, romaji: Query)) };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, MediaFormat.Movie, CancellationToken.None);

		await Assert.That(client.Formats.Single()).IsEqualTo(MediaFormat.Movie);
	}

	[Test]
	public async Task SearchWithoutAFormatPassesNoFormatToTheClient()
	{
		var client = new FakeAniListSearchClient { Response = Response(Media(id: 1U, romaji: Query)) };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, format: null, CancellationToken.None);

		await Assert.That(client.Formats.Single()).IsNull();
	}

	[Test]
	public async Task ANonNsfwChannelExcludesAdultResults()
	{
		var client = new FakeAniListSearchClient { Response = Response(Media(id: 1U, romaji: Query, isAdult: true)) };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target) { IncludeNsfw = false }, Query, format: null, CancellationToken.None);

		await Assert.That(target.Operations).IsEquivalentTo([FakeSearchMessageTarget.EditOperation], CollectionOrdering.Matching);
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.NoResults(Query));
	}

	[Test]
	public async Task AnNsfwChannelKeepsAdultResults()
	{
		var client = new FakeAniListSearchClient { Response = Response(Media(id: 1U, romaji: Query, isAdult: true)) };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target) { IncludeNsfw = true }, Query, format: null, CancellationToken.None);

		await Assert.That(target.Posts.Single().Title).StartsWith(Query);
	}

	[Test]
	public async Task AnEmptyPageIsReportedAsNoResults()
	{
		var client = new FakeAniListSearchClient { Response = MediaSearchResponse.Empty };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, format: null, CancellationToken.None);

		await Assert.That(target.Operations).IsEquivalentTo([FakeSearchMessageTarget.EditOperation], CollectionOrdering.Matching);
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.NoResults(Query));
	}

	[Test]
	public async Task ASingleCharacterQueryReachesAniList()
	{
		const string singleCharacterQuery = "K";
		var client = new FakeAniListSearchClient { Response = Response(Media(id: 1U, romaji: singleCharacterQuery)) };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), singleCharacterQuery, format: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsEqualTo(1);
	}

	[Test]
	public async Task AnEmptyQueryIsRejectedWithTheMinimumOfOneCharacter()
	{
		var client = new FakeAniListSearchClient();
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), "", format: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsZero();
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.QueryTooShort(1));
	}

	[Test]
	public async Task RendersFromTheStaticSearchDefaultWithoutQueryingTheRequester()
	{
		var client = new FakeAniListSearchClient { Response = Response(Media(id: 1U, romaji: Query)) };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, format: null, CancellationToken.None);

		var options = client.Options.Single();
		await Assert.That(options.HasFlag(RequestOptions.MediaFormat)).IsTrue();
		await Assert.That(options.HasFlag(RequestOptions.Tags)).IsTrue();
		await Assert.That(options.HasFlag(RequestOptions.Genres)).IsFalse();
		await Assert.That(options.HasFlag(RequestOptions.Studio)).IsFalse();
		await Assert.That(options.HasFlag(RequestOptions.Mangaka)).IsFalse();
		await Assert.That(client.UserIds.Single()).IsNull();
	}

	[Test]
	public async Task ResolvesTheRequesterTitleLanguageWhenBuildingResults()
	{
		var media = new SearchMedia
		{
			Id = 1U,
			Title = new() { Romaji = "Pocket Monster", English = Query },
			Url = "https://anilist.co/anime/1",
		};
		var client = new FakeAniListSearchClient { Response = Response(media, Requester(TitleLanguage.English)) };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, format: null, CancellationToken.None);

		await Assert.That(target.Posts.Single().Title).StartsWith(Query);
	}

	[Test]
	public async Task ClassifyMapsRateLimitToBusyAndEverythingElseToFailed()
	{
		var client = new FakeAniListSearchClient();
		await using var scope = await ServiceScope.CreateAsync(client);

		var busy = scope.Service.Classify(RateLimitException());
		var failed = scope.Service.Classify(new InvalidOperationException("boom"));

		await Assert.That(busy.UserMessage).IsEqualTo(SearchMessages.Busy("AniList"));
		await Assert.That(failed.UserMessage).IsEqualTo(SearchMessages.Failed("AniList"));
	}

	[Test]
	public async Task ARateLimitDuringSearchSurfacesTheBusyMessage()
	{
		var client = new FakeAniListSearchClient { SearchException = RateLimitException() };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, format: null, CancellationToken.None);

		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.Busy("AniList"));
	}

	[Test]
	public async Task TheSearchServiceHasNoDatabaseDependency()
	{
		var parameters = typeof(AniListMediaSearchService).GetConstructors().Single().GetParameters();
		var dependsOnDatabase = Array.Exists(parameters, static parameter => parameter.ParameterType.FullName?.Contains("DbContext", StringComparison.Ordinal) == true);

		await Assert.That(dependsOnDatabase).IsFalse();
	}

	private static GraphQLHttpRequestException RateLimitException()
	{
		using var response = new HttpResponseMessage();
		return new(HttpStatusCode.TooManyRequests, response.Headers, "");
	}

	private static SearchMedia Media(uint id, string romaji, bool isAdult = false) => new()
	{
		Id = id,
		Title = new() { Romaji = romaji },
		Url = $"https://anilist.co/anime/{id}",
		IsAdult = isAdult,
	};

	private static User Requester(TitleLanguage titleLanguage) => new()
	{
		Url = "https://anilist.co/user/1",
		Options = new() { TitleLanguage = titleLanguage },
		MediaListOptions = null,
	};

	private static MediaSearchResponse Response(SearchMedia media, User? user = null) => new()
	{
		Page = new() { Values = [media] },
		User = user,
	};

	private sealed class ServiceScope : IAsyncDisposable
	{
		private readonly MemoryCache _cache;

		public AniListMediaSearchService Service { get; }

		private ServiceScope(MemoryCache cache, AniListMediaSearchService service)
		{
			this._cache = cache;
			this.Service = service;
		}

		public static Task<ServiceScope> CreateAsync(FakeAniListSearchClient client)
		{
			var cache = new MemoryCache(new MemoryCacheOptions());
			var time = new FakeTimeProvider(Start);
			var picker = new SearchPicker(cache, time, NullLogger<SearchPicker>.Instance);
			var orchestrator = new SearchOrchestrator(picker, time, NullLogger<SearchOrchestrator>.Instance);
			return Task.FromResult(new ServiceScope(cache, new(client, NullLogger<AniListMediaSearchService>.Instance, orchestrator)));
		}

		public ValueTask DisposeAsync()
		{
			this._cache.Dispose();
			return ValueTask.CompletedTask;
		}
	}
}
