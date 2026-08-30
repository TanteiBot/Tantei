// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class SearchLogTests
{
	private const string SearchId = "0123456789abcdef0123456789abcdef";
	private const string AvatarUrl = "https://cdn.discordapp.com/avatars/1/secret.png";

	[Test]
	public async Task PermissionDenialIsAnInformationEventBecauseNothingFailed()
	{
		var logger = new RecordingLogger<SearchLogTests>();

		logger.PermissionDenied();

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Information);
		await Assert.That(entry.EventId.Id).IsEqualTo(1);
		await Assert.That(entry.EventId.Name).IsEqualTo("PermissionDenied");
	}

	[Test]
	public async Task EachFailureCauseHasItsOwnEventAtTheAgreedLevel()
	{
		var logger = new RecordingLogger<SearchLogTests>();
		var failure = new InvalidOperationException("boom");

		logger.PermissionDenied();
		logger.OfficialApiForbidden();
		logger.RateLimiterQueueRejected();
		logger.SearchFailed(failure);
		logger.PublicPostForbidden();
		logger.PublicPostFailed(failure);
		logger.PickerInteractionFailed(failure);
		logger.TerminalStatePushFailed(failure);

		await Assert.That(Catalog(logger)).IsEquivalentTo(
			[
				"1 PermissionDenied Information",
				"2 OfficialApiForbidden Error",
				"3 RateLimiterQueueRejected Warning",
				"4 SearchFailed Error",
				"5 PublicPostForbidden Warning",
				"6 PublicPostFailed Error",
				"7 PickerInteractionFailed Error",
				"8 TerminalStatePushFailed Warning",
			],
			TUnit.Assertions.Enums.CollectionOrdering.Matching);
	}

	[Test]
	public async Task OnlyTheCausesThatCarryAnExceptionRecordOne()
	{
		var logger = new RecordingLogger<SearchLogTests>();
		var failure = new InvalidOperationException("boom");

		logger.OfficialApiForbidden();
		logger.RateLimiterQueueRejected();
		logger.PublicPostForbidden();
		logger.SearchFailed(failure);
		logger.PublicPostFailed(failure);
		logger.PickerInteractionFailed(failure);
		logger.TerminalStatePushFailed(failure);

		await Assert.That(logger.Entries.Select(static entry => entry.Exception is not null)).IsEquivalentTo(
			[false, false, false, true, true, true, true],
			TUnit.Assertions.Enums.CollectionOrdering.Matching);
	}

	[Test]
	public async Task EveryCompletedSearchReportsOneOutcomeFromTheAgreedVocabulary()
	{
		var logger = new RecordingLogger<SearchLogTests>();

		foreach (var outcome in Enum.GetValues<SearchOutcomeKind>())
		{
			logger.SearchCompleted(outcome, floorSurvivorCount: 0, resultCount: 0);
		}

		await Assert.That(Outcomes(logger)).IsEquivalentTo(
			["NoResults", "TypeFilterEmpty", "AutoPosted", "PickerOpened"],
			TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(Catalog(logger).Distinct(StringComparer.Ordinal)).IsEquivalentTo(["9 SearchCompleted Information"]);
	}

	[Test]
	public async Task ACompletedSearchCarriesItsResultCounts()
	{
		var logger = new RecordingLogger<SearchLogTests>();
		const int floorSurvivors = 7;
		const int results = 4;

		logger.SearchCompleted(SearchOutcomeKind.PickerOpened, floorSurvivors, results);

		await Assert.That(Field(logger.Single(), "FloorSurvivorCount")).IsEqualTo("7");
		await Assert.That(Field(logger.Single(), "ResultCount")).IsEqualTo("4");
	}

	[Test]
	public async Task APickerEndsWithOneOutcomeFromTheAgreedVocabulary()
	{
		var logger = new RecordingLogger<SearchLogTests>();

		foreach (var reason in Enum.GetValues<PickerTerminalReason>())
		{
			logger.PickerEnded(reason, selectedMediaId: null);
		}

		await Assert.That(Outcomes(logger)).IsEquivalentTo(
			["Picked", "Cancelled", "InactivityTimeout", "AbsoluteTimeout", "PostFailed", "InteractionFailed"],
			TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(Catalog(logger).Distinct(StringComparer.Ordinal)).IsEquivalentTo(["10 PickerEnded Information"]);
	}

	[Test]
	public async Task OnlyAPickThatSucceededRecordsTheSelectedMediaId()
	{
		var logger = new RecordingLogger<SearchLogTests>();
		const uint selectedMediaId = 1535U;

		logger.PickerEnded(PickerTerminalReason.Picked, selectedMediaId);
		logger.PickerEnded(PickerTerminalReason.Cancelled, selectedMediaId: null);

		await Assert.That(Field(logger.Entries[0], "SelectedMediaId")).IsEqualTo("1535");
		await Assert.That(Field(logger.Entries[1], "SelectedMediaId")).IsNull();
	}

	[Test]
	public async Task AnUnavailablePickerIsANormalInformationOutcome()
	{
		var logger = new RecordingLogger<SearchLogTests>();

		logger.PickerUnavailable();

		await Assert.That(Catalog(logger)).IsEquivalentTo(["11 PickerUnavailable Information"]);
	}

	[Test]
	public async Task TheSearchScopeCarriesOnlyTheAgreedScalarContext()
	{
		var logger = new RecordingLogger<SearchLogTests>();
		var context = new PickerSearchContext(
			Query: "Monster",
			MediaKind: PickerMediaKind.Anime,
			MediaTypeFilter: "TV",
			DiscordUserId: 1UL,
			RequesterDisplayName: "Requester",
			RequesterAvatarUrl: AvatarUrl,
			GuildId: 2UL,
			ChannelId: 3UL,
			InvokedAt: DateTimeOffset.UnixEpoch);

		using var scope = logger.SearchScope(SearchId, context);

		await Assert.That(Fields(logger)).IsEquivalentTo(
			[
				"SearchId=" + SearchId,
				"Query=Monster",
				"MediaKind=Anime",
				"TypeFilter=TV",
				"DiscordUserId=1",
				"DiscordDisplayName=Requester",
				"GuildId=2",
				"ChannelId=3",
			],
			TUnit.Assertions.Enums.CollectionOrdering.Matching);
	}

	[Test]
	public async Task TheSearchScopeNeverCarriesTheRequesterAvatar()
	{
		var logger = new RecordingLogger<SearchLogTests>();
		var context = new PickerSearchContext(
			Query: "Monster",
			MediaKind: PickerMediaKind.Anime,
			MediaTypeFilter: null,
			DiscordUserId: 1UL,
			RequesterDisplayName: "Requester",
			RequesterAvatarUrl: AvatarUrl,
			GuildId: 2UL,
			ChannelId: 3UL,
			InvokedAt: DateTimeOffset.UnixEpoch);

		using var scope = logger.SearchScope(SearchId, context);

		await Assert.That(logger.Scopes.Single()!.ToString()).DoesNotContain(AvatarUrl);
	}

	[Test]
	public async Task AnUnavailablePickerScopeCannotClaimTheOriginalSearchContext()
	{
		var logger = new RecordingLogger<SearchLogTests>();

		using var scope = logger.PickerInteractionScope(SearchId, discordUserId: 7UL, "Someone", guildId: 8UL, channelId: 9UL);

		await Assert.That(Fields(logger)).IsEquivalentTo(
			[
				"SearchId=" + SearchId,
				"DiscordUserId=7",
				"DiscordDisplayName=Someone",
				"GuildId=8",
				"ChannelId=9",
			],
			TUnit.Assertions.Enums.CollectionOrdering.Matching);
	}

	private static IEnumerable<string> Fields(RecordingLogger<SearchLogTests> logger) =>
		((IReadOnlyList<KeyValuePair<string, object?>>)logger.Scopes.Single()!).Select(static field =>
			field.Key + "=" + Convert.ToString(field.Value, CultureInfo.InvariantCulture));

	private static IEnumerable<string> Outcomes(RecordingLogger<SearchLogTests> logger) =>
		logger.Entries.Select(static entry => Field(entry, "Outcome") ?? string.Empty);

	private static string? Field(RecordedLogEntry entry, string name) =>
		entry.State.SingleOrDefault(field => string.Equals(field.Key, name, StringComparison.Ordinal)).Value?.ToString();

	private static IEnumerable<string> Catalog(RecordingLogger<SearchLogTests> logger) =>
		logger.Entries.Select(static entry =>
			string.Create(CultureInfo.InvariantCulture, $"{entry.EventId.Id} {entry.EventId.Name} {entry.Level}"));
}
