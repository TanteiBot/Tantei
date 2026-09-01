// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using Polly.RateLimiting;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class MalSearchServiceTests
{
	private const string Query = "Monster";
	private const int PickerRowCount = 2;
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task AMissingChannelPermissionEndsTheSearchBeforeValidationAndBeforeMyAnimeList()
	{
		var client = new FakeMyAnimeListSearchClient();
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target) { CanPostEmbed = false, }, "!", mediaType: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsZero();
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.MissingPermissions);
		await Assert.That(EventNames(scope)).IsEquivalentTo(["PermissionDenied"]);
	}

	[Test]
	public async Task AQueryShorterThanThreeTextElementsIsRejectedBeforeMyAnimeList()
	{
		var client = new FakeMyAnimeListSearchClient();
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), "\U0001F1EF\U0001F1F5こ", mediaType: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsZero();
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.QueryTooShort);
	}

	[Test]
	public async Task AQueryWithoutLettersOrDigitsIsRejectedBeforeMyAnimeList()
	{
		var client = new FakeMyAnimeListSearchClient();
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), "!!!", mediaType: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsZero();
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.QueryWithoutLettersOrDigits);
	}

	[Test]
	[Arguments(false)]
	[Arguments(true)]
	public async Task TheChannelDecidesWhetherMyAnimeListIsAskedForNsfwResults(bool includeNsfw)
	{
		var client = new FakeMyAnimeListSearchClient { AnimeResults = AnimeResults(Anime(1, Query)), };
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target) { IncludeNsfw = includeNsfw, }, Query, mediaType: null, CancellationToken.None);

		await Assert.That(client.NsfwFlags).IsEquivalentTo([includeNsfw]);
		await Assert.That(client.Queries).IsEquivalentTo([Query]);
	}

	[Test]
	public async Task AnEmptyMyAnimeListSnapshotReportsNoResults()
	{
		var client = new FakeMyAnimeListSearchClient();
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsEqualTo(1);
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.NoResults(Query));
		await Assert.That(Outcomes(scope)).IsEquivalentTo([nameof(SearchOutcomeKind.NoResults)]);
	}

	[Test]
	public async Task ARelevanceFloorThatRemovesEveryRowReportsNoResults()
	{
		var client = new FakeMyAnimeListSearchClient { AnimeResults = AnimeResults(Anime(1, "Something else")), };
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.NoResults(Query));
		await Assert.That(Outcomes(scope)).IsEquivalentTo([nameof(SearchOutcomeKind.NoResults)]);
	}

	[Test]
	public async Task ATypeFilterThatRemovesEveryFloorSurvivorSaysSo()
	{
		var client = new FakeMyAnimeListSearchClient { AnimeResults = AnimeResults(Anime(1, Query)), };
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, AnimeMediaType.Movie, CancellationToken.None);

		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.TypeFilterEmpty(Query));
		await Assert.That(Outcomes(scope)).IsEquivalentTo([nameof(SearchOutcomeKind.TypeFilterEmpty)]);
	}

	[Test]
	public async Task ASoleSurvivingResultIsPostedPubliclyBeforeTheEphemeralIsDeleted()
	{
		var client = new FakeMyAnimeListSearchClient { AnimeResults = AnimeResults(Anime(1, Query)), };
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsEqualTo(1);
		await Assert.That(target.Operations).IsEquivalentTo(
			[FakeSearchMessageTarget.PostOperation, FakeSearchMessageTarget.DeleteOperation],
			TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(target.Posts.Single().Title).IsEqualTo(Query);
		await Assert.That(Outcomes(scope)).IsEquivalentTo([nameof(SearchOutcomeKind.AutoPosted)]);
	}

	[Test]
	public async Task ASinglePrimaryTitleMatchIsPostedOutOfALargerResultSet()
	{
		var client = new FakeMyAnimeListSearchClient
		{
			AnimeResults = AnimeResults(Anime(1, $"{Query} Rage"), Anime(2, Query), Anime(3, $"{Query} 2")),
		};
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(target.Posts.Single().Title).IsEqualTo(Query);
		await Assert.That(target.Operations).DoesNotContain(FakeSearchMessageTarget.EditOperation);
	}

	[Test]
	public async Task AFailingPublicPostLeavesOneTerminalEphemeralErrorAndNoDelete()
	{
		var client = new FakeMyAnimeListSearchClient { AnimeResults = AnimeResults(Anime(1, Query)), };
		var target = new FakeSearchMessageTarget { PostException = new InvalidOperationException("denied"), };
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(target.Operations).IsEquivalentTo(
			[FakeSearchMessageTarget.PostOperation, FakeSearchMessageTarget.EditOperation],
			TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.PostFailed);
		await Assert.That(target.Edits[0].Rows).IsEmpty();
		await Assert.That(EventNames(scope)).Contains("PublicPostFailed");
	}

	[Test]
	public async Task SeveralSurvivingResultsEditTheEphemeralIntoAPickerAfterOneApiCall()
	{
		var client = new FakeMyAnimeListSearchClient { AnimeResults = AnimeResults(Anime(1, $"{Query} Rage"), Anime(2, $"{Query} 2")), };
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsEqualTo(1);
		await Assert.That(target.Operations).IsEquivalentTo([FakeSearchMessageTarget.EditOperation], TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(target.Edits.Single().Rows.Count).IsEqualTo(PickerRowCount);
		await Assert.That(Outcomes(scope)).IsEquivalentTo([nameof(SearchOutcomeKind.PickerOpened)]);
		var select = (DiscordSelectComponent)target.Edits.Single().Rows[0][0];
		var parsed = PickerCustomId.TryParse(select.CustomId, out var customId);
		var loggedSearchId = ((IReadOnlyList<KeyValuePair<string, object?>>)scope.Logger.Scopes.Single()!)
			.Single(static field => string.Equals(field.Key, "SearchId", StringComparison.Ordinal))
			.Value;
		await Assert.That(parsed).IsTrue();
		await Assert.That(loggedSearchId).IsEqualTo(customId.SearchId);
	}

	[Test]
	public async Task AMangaSearchRunsTheSameFlowThroughTheMangaEndpoint()
	{
		var client = new FakeMyAnimeListSearchClient { MangaResults = MangaResults(Manga(1, Query)), };
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchMangaAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(client.CallCount).IsEqualTo(1);
		await Assert.That(target.Posts.Single().Title).IsEqualTo(Query);
	}

	[Test]
	public async Task AnOfficialApiForbiddenIsReportedAsBusy()
	{
		var client = new FakeMyAnimeListSearchClient { SearchException = new HttpRequestException("forbidden", inner: null, HttpStatusCode.Forbidden), };
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.Busy);
		await Assert.That(EventNames(scope)).IsEquivalentTo(["OfficialApiForbidden"]);
	}

	[Test]
	public async Task AFullRateLimiterQueueIsReportedAsBusyUnderItsOwnEvent()
	{
		var client = new FakeMyAnimeListSearchClient { SearchException = new RateLimiterRejectedException(), };
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.Busy);
		await Assert.That(EventNames(scope)).IsEquivalentTo(["RateLimiterQueueRejected"]);
	}

	[Test]
	public async Task AnyOtherMyAnimeListFailureStaysGeneric()
	{
		var client = new FakeMyAnimeListSearchClient { SearchException = new HttpRequestException("gateway"), };
		var target = new FakeSearchMessageTarget();
		using var scope = new ServiceScope(client);

		await scope.Service.SearchAnimeAsync(new FakeSearchInvocation(target), Query, mediaType: null, CancellationToken.None);

		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.Failed);
		await Assert.That(EventNames(scope)).IsEquivalentTo(["SearchFailed"]);
	}

	private static IEnumerable<string> EventNames(ServiceScope scope) => scope.Logger.Entries.Select(static entry => entry.EventId.Name ?? string.Empty);

	private static IEnumerable<string> Outcomes(ServiceScope scope) =>
		scope.Logger.Entries
			 .Where(static entry => string.Equals(entry.EventId.Name, "SearchCompleted", StringComparison.Ordinal))
			 .Select(static entry => entry.State
										  .SingleOrDefault(static field => string.Equals(field.Key, "Outcome", StringComparison.Ordinal))
										  .Value?.ToString() ?? string.Empty);

	private static AnimeSearchResult[] AnimeResults(params AnimeSearchResult[] results) => results;

	private static MangaSearchResult[] MangaResults(params MangaSearchResult[] results) => results;

	private static AnimeSearchResult Anime(int id, string title) => new()
	{
		Id = (uint)id,
		PrimaryTitle = title,
		MediaType = AnimeMediaType.TV,
		Status = AnimeAiringStatus.Unknown,
		Episodes = 0U,
		ListUserCount = (uint)id,
		Genres = [],
	};

	private static MangaSearchResult Manga(int id, string title) => new()
	{
		Id = (uint)id,
		PrimaryTitle = title,
		MediaType = MangaMediaType.Manga,
		Status = MangaPublishingStatus.Unknown,
		Chapters = 0U,
		Volumes = 0U,
		ListUserCount = (uint)id,
		Genres = [],
	};

	private sealed class ServiceScope : IDisposable
	{
		private readonly MemoryCache _cache = new(new MemoryCacheOptions());

		public RecordingLogger<MalSearchService> Logger { get; } = new();

		public MalSearchService Service { get; }

		public ServiceScope(FakeMyAnimeListSearchClient client)
		{
			var timeProvider = new ManualTimeProvider(Start);
			var picker = new MalSearchPicker(this._cache, timeProvider, NullLogger<MalSearchPicker>.Instance);
			this.Service = new(client, picker, timeProvider, this.Logger);
		}

		public void Dispose() => this._cache.Dispose();
	}
}
