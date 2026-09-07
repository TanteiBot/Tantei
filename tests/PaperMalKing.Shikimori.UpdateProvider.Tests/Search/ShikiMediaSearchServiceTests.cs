// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using GraphQL.Client.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.Shikimori.UpdateProvider.Search;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;
using PaperMalKing.UpdatesProviders.Base.Search;
using TUnit.Assertions.Enums;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests.Search;

public sealed class ShikiMediaSearchServiceTests
{
	private const string Query = "Monster";
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task SearchAnimeAutoPostsAnExactPrimaryMatchAndDeletesTheEphemeral()
	{
		var client = new FakeShikiSearchClient { AnimeResults = [Anime(id: 1UL, name: Query)] };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, kind: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsEqualTo(1);
		await Assert.That(target.Operations).IsEquivalentTo(
			[FakeSearchMessageTarget.PostOperation, FakeSearchMessageTarget.DeleteOperation],
			CollectionOrdering.Matching);
		await Assert.That(target.Posts.Single().Title).StartsWith(Query);
	}

	[Test]
	public async Task SearchMangaQueriesTheMangaPath()
	{
		var client = new FakeShikiSearchClient { MangaResults = [Manga(id: 1UL, name: Query)] };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchMangaAsync(new FakeSearchInvocation(target), Query, kind: null, CancellationToken.None);

		await Assert.That(client.MangaKinds).Count().IsEqualTo(1);
		await Assert.That(target.Posts.Single().Title).StartsWith(Query);
	}

	[Test]
	public async Task KindFilterIsForwardedToTheClient()
	{
		var client = new FakeShikiSearchClient { AnimeResults = [Anime(id: 1UL, name: Query)] };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, AnimeKind.Movie, CancellationToken.None);

		await Assert.That(client.AnimeKinds.Single()).IsEqualTo(AnimeKind.Movie);
	}

	[Test]
	public async Task ANonNsfwChannelExcludesAdultAnime()
	{
		var client = new FakeShikiSearchClient { AnimeResults = [Anime(id: 1UL, name: Query, rating: "rx")] };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target) { IncludeNsfw = false }, Query, kind: null, CancellationToken.None);

		await Assert.That(client.IncludeNsfws.Single()).IsFalse();
		await Assert.That(target.Operations).IsEquivalentTo([FakeSearchMessageTarget.EditOperation], CollectionOrdering.Matching);
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.NoResults(Query));
	}

	[Test]
	public async Task AnNsfwChannelKeepsAdultAnime()
	{
		var client = new FakeShikiSearchClient { AnimeResults = [Anime(id: 1UL, name: Query, rating: "rx")] };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target) { IncludeNsfw = true }, Query, kind: null, CancellationToken.None);

		await Assert.That(client.IncludeNsfws.Single()).IsTrue();
		await Assert.That(target.Posts.Single().Title).StartsWith(Query);
	}

	[Test]
	public async Task NoMatchingTitleReportsNoResults()
	{
		var client = new FakeShikiSearchClient { AnimeResults = [Anime(id: 1UL, name: "Something Else")] };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, kind: null, CancellationToken.None);

		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.NoResults(Query));
	}

	[Test]
	public async Task RendersTheDefaultNativeTitleWithoutRussian()
	{
		var client = new FakeShikiSearchClient { AnimeResults = [Anime(id: 1UL, name: Query, russian: "Монстр")] };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, kind: null, CancellationToken.None);

		await Assert.That(target.Posts.Single().Title).StartsWith(Query);
	}

	[Test]
	public async Task ClassifyMapsRateLimitToBusyAndEverythingElseToFailed()
	{
		var client = new FakeShikiSearchClient();
		await using var scope = await ServiceScope.CreateAsync(client);

		var busy = scope.Service.Classify(RateLimitException());
		var failed = scope.Service.Classify(new InvalidOperationException("boom"));

		await Assert.That(busy.UserMessage).IsEqualTo(SearchMessages.Busy("Shikimori"));
		await Assert.That(failed.UserMessage).IsEqualTo(SearchMessages.Failed("Shikimori"));
	}

	[Test]
	public async Task ARateLimitDuringSearchSurfacesTheBusyMessage()
	{
		var client = new FakeShikiSearchClient { SearchException = RateLimitException() };
		await using var scope = await ServiceScope.CreateAsync(client);
		var target = new FakeSearchMessageTarget();

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, kind: null, CancellationToken.None);

		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.Busy("Shikimori"));
	}

	[Test]
	public async Task TheSearchServiceHasNoDatabaseDependency()
	{
		var parameters = typeof(ShikiMediaSearchService).GetConstructors().Single().GetParameters();
		var dependsOnDatabase = Array.Exists(parameters, static parameter => parameter.ParameterType.FullName?.Contains("DbContext", StringComparison.Ordinal) == true);

		await Assert.That(dependsOnDatabase).IsFalse();
	}

	private static GraphQLHttpRequestException RateLimitException()
	{
		using var response = new HttpResponseMessage();
		return new(HttpStatusCode.TooManyRequests, response.Headers, "");
	}

	private static AnimeSearchMedia Anime(ulong id, string name, string? russian = null, string? rating = null) => new()
	{
		Id = id,
		Name = name,
		RussianName = russian,
		Synonyms = [],
		Kind = "tv",
		Status = "released",
		Rating = rating,
		StatusesStats = [],
		Url = $"https://shikimori.io/animes/{id}",
	};

	private static MangaSearchMedia Manga(ulong id, string name) => new()
	{
		Id = id,
		Name = name,
		Synonyms = [],
		Kind = "manga",
		Status = "ongoing",
		StatusesStats = [],
		Url = $"https://shikimori.io/mangas/{id}",
	};

	private sealed class ServiceScope : IAsyncDisposable
	{
		private readonly MemoryCache _cache;

		public ShikiMediaSearchService Service { get; }

		private ServiceScope(MemoryCache cache, ShikiMediaSearchService service)
		{
			this._cache = cache;
			this.Service = service;
		}

		public static Task<ServiceScope> CreateAsync(FakeShikiSearchClient client)
		{
			var cache = new MemoryCache(new MemoryCacheOptions());
			var time = new FakeTimeProvider(Start);
			var picker = new SearchPicker(cache, time, NullLogger<SearchPicker>.Instance);
			var orchestrator = new SearchOrchestrator(picker, time, NullLogger<SearchOrchestrator>.Instance);
			return Task.FromResult(new ServiceScope(cache, new(client, orchestrator)));
		}

		public ValueTask DisposeAsync()
		{
			this._cache.Dispose();
			return ValueTask.CompletedTask;
		}
	}
}
