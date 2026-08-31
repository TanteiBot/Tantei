// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "The task sources are controlled by the test fake")]
public sealed class MalSearchPickerTests
{
	private const string UnavailablePhrase = "no longer available";
	private const int ResultCountAcrossTwoPages = 26;
	private const int MinutesBeforeAbsoluteExpiry = 13;
	private const string PickerEndedEvent = "PickerEnded";
	private const string PostOperation = "post";
	private const string DeleteOperation = "delete";
	private const string EditOperation = "edit";
	private static readonly Guid SearchId = SearchTestIdentity.Value;
	private static readonly TimeSpan InactivityLifetime = TimeSpan.FromSeconds(90);
	private static readonly TimeSpan AbsoluteLifetime = TimeSpan.FromMinutes(14);
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task AnUnrecognizedInteractionRemainsUntouched()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var interaction = new FakePickerInteraction("another-feature:next");

		var routed = await picker.HandleAsync(interaction);

		await Assert.That(routed).IsFalse();
		await Assert.That(interaction.RecognizedCount).IsEqualTo(0);
		await Assert.That(interaction.Replacements).IsEmpty();
	}

	[Test]
	public async Task AMalformedPickerInteractionIsARecognizedUnavailableOutcome()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var interaction = new FakePickerInteraction("mal:search:not-a-picker");

		var routed = await picker.HandleAsync(interaction);

		await Assert.That(routed).IsTrue();
		await Assert.That(interaction.Replacements.Single().Content).Contains(UnavailablePhrase);
	}

	[Test]
	public async Task AnUnopenedPickerIsARecognizedUnavailableOutcome()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var interaction = new FakePickerInteraction(PickerCustomId.Create(SearchId, PickerAction.Next));

		var routed = await picker.HandleAsync(interaction);

		await Assert.That(routed).IsTrue();
		await Assert.That(interaction.Replacements).HasSingleItem();
		await Assert.That(interaction.Replacements[0].Content).IsEqualTo("This search is no longer available. Run the command again.");
		await Assert.That(interaction.Replacements[0].Rows).IsEmpty();
	}

	[Test]
	public async Task AnOverduePickerOpensInAControlFreeExpiredState()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start + AbsoluteLifetime);
		var picker = CreatePicker(cache, time);

		var opened = Open(picker, new FakePickerMessageTarget(), resultCount: 1);

		await Assert.That(opened.View.Content).Contains("expired");
		await Assert.That(opened.View.Rows).IsEmpty();
	}

	[Test]
	public async Task InitialDeliveryFailureLeavesThePickerUnavailable()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget { EditException = new InvalidOperationException("delivery failed"), };
		var failed = false;

		try
		{
			await picker.OpenAsync(
				SearchId,
				[Result(1)],
				Context(),
				target);
		}
		catch (InvalidOperationException)
		{
			failed = true;
		}

		await Assert.That(failed).IsTrue();
		await Assert.That(target.LastEditCancellationToken.IsCancellationRequested).IsTrue();
		var interaction = new FakePickerInteraction(PickerCustomId.Create(SearchId, PickerAction.Next));
		await picker.HandleAsync(interaction);
		await Assert.That(interaction.Replacements.Single().Content).Contains(UnavailablePhrase);
		time.Advance(AbsoluteLifetime);
		await Assert.That(target.Edits).HasSingleItem();
	}

	[Test]
	public async Task ExpiryCancelsAHangingInitialDeliveryWithoutOpeningThePicker()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget { PauseEditNumber = 1, };
		var opening = picker.OpenAsync(
			SearchId,
			[Result(1)],
			Context(),
			target);
		await target.EditStarted.Task;

		time.Advance(InactivityLifetime);
		await opening;

		var interaction = Pick(SearchId);
		await picker.HandleAsync(interaction);
		await Assert.That(interaction.RecognizedCount).IsEqualTo(1);
		await Assert.That(target.Operations).DoesNotContain(PostOperation);
		await Assert.That(target.CompletedEditCount).IsEqualTo(0);
		target.AllowEdit.SetResult();
	}

	[Test]
	public async Task EvictionDuringOpeningCannotReactivateThePicker()
	{
		using var cache = new EvictingMemoryCache();
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget();

		await picker.OpenAsync(
			SearchId,
			[Result(1)],
			Context(),
			target);
		var interaction = Pick(SearchId);
		await picker.HandleAsync(interaction);

		await Assert.That(interaction.Replacements.Single().Content).Contains(UnavailablePhrase);
		await Assert.That(target.Operations).DoesNotContain(PostOperation);
	}

	[Test]
	public async Task PageInteractionUpdatesThePickerWithinTheSnapshot()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var opened = Open(picker, new FakePickerMessageTarget(), ResultCountAcrossTwoPages);
		var interaction = new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Next));

		await picker.HandleAsync(interaction);

		var select = (DiscordSelectComponent)interaction.Replacements.Single().Rows[0][0];
		var page = (DiscordButtonComponent)interaction.Replacements.Single().Rows[1][1];
		await Assert.That(select.Options).HasSingleItem();
		await Assert.That(page.Label).IsEqualTo("Page 2/2");
	}

	[Test]
	public async Task InteractionFromAnyoneOtherThanTheRequesterIsIgnored()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var target = new FakePickerMessageTarget();
		var opened = Open(picker, target);
		var interaction = Pick(opened.SearchId) with { DiscordUserId = 99UL, };

		await picker.HandleAsync(interaction);

		await Assert.That(interaction.RecognizedCount).IsEqualTo(1);
		await Assert.That(target.Operations).IsEmpty();
	}

	[Test]
	public async Task PickPostsBeforeDeletingTheOriginal()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var target = new FakePickerMessageTarget();
		var opened = Open(picker, target);
		var interaction = Pick(opened.SearchId);

		await picker.HandleAsync(interaction);

		await Assert.That(target.Operations).IsEquivalentTo([PostOperation, DeleteOperation], TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(interaction.RecognizedCount).IsEqualTo(1);
	}

	[Test]
	public async Task SuccessfulPickEndsBeforeItsDeleteCompletes()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget { PauseDelete = true, };
		var opened = Open(picker, target);
		var pick = picker.HandleAsync(Pick(opened.SearchId));
		await target.DeleteStarted.Task;

		var repeatedPick = Pick(opened.SearchId);
		await picker.HandleAsync(repeatedPick);
		await Assert.That(repeatedPick.RecognizedCount).IsEqualTo(1);
		time.Advance(AbsoluteLifetime);
		await Assert.That(target.Edits).IsEmpty();
		target.AllowDelete.SetResult();
		await pick;
		await Assert.That(target.Operations).IsEquivalentTo([PostOperation, DeleteOperation], TUnit.Assertions.Enums.CollectionOrdering.Matching);
	}

	[Test]
	public async Task PostFailureLeavesOneControlFreeTerminalError()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var target = new FakePickerMessageTarget { PostException = new InvalidOperationException("denied"), };
		var opened = Open(picker, target);

		await picker.HandleAsync(Pick(opened.SearchId));
		await picker.HandleAsync(Pick(opened.SearchId));

		await Assert.That(target.Operations).IsEquivalentTo([PostOperation, EditOperation], TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(target.Edits).HasSingleItem();
		await Assert.That(target.Edits[0].Rows).IsEmpty();
		await Assert.That(target.Edits[0].Content).Contains("couldn't post");
	}

	[Test]
	public async Task ConcurrentPickAndCancelHaveOneTerminalWinner()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var target = new FakePickerMessageTarget { PausePost = true, };
		var opened = Open(picker, target);
		var pick = picker.HandleAsync(Pick(opened.SearchId));
		await target.PostStarted.Task;
		var cancelInteraction = new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Cancel));
		var cancel = picker.HandleAsync(cancelInteraction);

		target.AllowPost.SetResult();
		await Task.WhenAll(pick, cancel);

		await Assert.That(target.Operations).IsEquivalentTo([PostOperation, DeleteOperation], TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(cancelInteraction.Replacements).IsEmpty();
		await Assert.That(cancelInteraction.RecognizedCount).IsEqualTo(1);
	}

	[Test]
	public async Task InactivityTimeoutWinsWhileAPublicPostHangs()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget { PausePost = true, };
		var opened = Open(picker, target);
		var pick = picker.HandleAsync(Pick(opened.SearchId));
		await target.PostStarted.Task;

		time.Advance(InactivityLifetime);
		target.AllowPost.SetResult();
		await pick;

		await Assert.That(target.Operations).IsEquivalentTo([PostOperation, EditOperation], TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(target.CompletedPostCount).IsEqualTo(0);
		await Assert.That(target.Edits.Single().Content).Contains("idled out");
	}

	[Test]
	public async Task TimeoutThatWinsThePickRacePreventsThePost()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget();
		var opened = Open(picker, target);
		time.Advance(InactivityLifetime);
		var pick = Pick(opened.SearchId);

		await picker.HandleAsync(pick);

		await Assert.That(target.Operations).DoesNotContain(PostOperation);
		await Assert.That(target.Edits).HasSingleItem();
		await Assert.That(pick.RecognizedCount).IsEqualTo(1);
	}

	[Test]
	public async Task InactivityTimeoutEndsThePickerWhileItsTerminalEditHangs()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget { PauseEditNumber = 2, };
		var opened = Open(picker, target);

		time.Advance(InactivityLifetime);
		await target.EditStarted.Task;

		var interaction = Pick(opened.SearchId);
		await picker.HandleAsync(interaction);
		await Assert.That(interaction.RecognizedCount).IsEqualTo(1);
		await Assert.That(target.Operations).DoesNotContain(PostOperation);
		target.AllowEdit.SetResult();
	}

	[Test]
	public async Task InactivityTimeoutEndsThePickerWhileAPageEditHangs()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget();
		var opened = Open(picker, target);
		var interaction = new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Page)) { PauseReplacement = true, };
		var page = picker.HandleAsync(interaction);
		await interaction.ReplacementStarted.Task;

		time.Advance(InactivityLifetime);

		var pick = Pick(opened.SearchId);
		await picker.HandleAsync(pick);
		await Assert.That(pick.RecognizedCount).IsEqualTo(1);
		await Assert.That(target.Operations).DoesNotContain(PostOperation);
		interaction.AllowReplacement.SetResult();
		await page;
	}

	[Test]
	public async Task InactivityClockSlidesOnEveryRecognizedInteraction()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget();
		var opened = Open(picker, target);
		time.Advance(TimeSpan.FromSeconds(80));
		await picker.HandleAsync(new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Page)));

		time.Advance(TimeSpan.FromSeconds(80));
		await Assert.That(target.Edits).IsEmpty();
		time.Advance(TimeSpan.FromSeconds(11));

		await Assert.That(target.Edits).HasSingleItem();
		await Assert.That(target.Edits[0].Content).Contains("idled out");
	}

	[Test]
	public async Task AbsoluteClockNeverSlides()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var picker = CreatePicker(cache, time);
		var target = new FakePickerMessageTarget();
		var opened = Open(picker, target);
		for (var minute = 0; minute < MinutesBeforeAbsoluteExpiry; minute++)
		{
			time.Advance(TimeSpan.FromMinutes(1));
			await picker.HandleAsync(new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Page)));
		}

		time.Advance(TimeSpan.FromMinutes(1));

		await Assert.That(target.Edits).HasSingleItem();
		await Assert.That(target.Edits[0].Content).Contains("expired");
	}

	[Test]
	public async Task AbsoluteTimeoutEndsThePickerWhileAPublicPostHangs()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var time = new ManualTimeProvider(Start);
		var logger = new RecordingLogger<MalSearchPicker>();
		var picker = CreatePicker(cache, time, logger);
		var target = new FakePickerMessageTarget { PausePost = true, };
		var opened = Open(picker, target);
		for (var minute = 0; minute < MinutesBeforeAbsoluteExpiry; minute++)
		{
			time.Advance(TimeSpan.FromMinutes(1));
			await picker.HandleAsync(new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Page)));
		}

		var pick = picker.HandleAsync(Pick(opened.SearchId));
		await target.PostStarted.Task;
		time.Advance(TimeSpan.FromMinutes(1));

		var repeatedPick = Pick(opened.SearchId);
		await picker.HandleAsync(repeatedPick);
		await Assert.That(repeatedPick.RecognizedCount).IsEqualTo(1);
		target.AllowPost.SetResult();
		await pick;
		await Assert.That(target.Operations).IsEquivalentTo([PostOperation, EditOperation], TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(target.CompletedPostCount).IsEqualTo(0);
		await Assert.That(Events(logger, PickerEndedEvent)).IsEquivalentTo(["AbsoluteTimeout"]);
	}

	[Test]
	public async Task AcknowledgementFailureTerminatesAndReportsThroughTheComponent()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var target = new FakePickerMessageTarget();
		var opened = Open(picker, target);
		var interaction = new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Next))
		{
			OutcomeFailuresRemaining = 1,
		};

		await picker.HandleAsync(interaction);
		await picker.HandleAsync(Pick(opened.SearchId));

		await Assert.That(interaction.Replacements).HasSingleItem();
		await Assert.That(interaction.Replacements[0].Content).Contains("Something went wrong");
		await Assert.That(target.Operations).IsEmpty();
	}

	[Test]
	public async Task UnexpectedInteractionErrorTerminatesAndReportsThroughTheComponent()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var picker = CreatePicker(cache, new ManualTimeProvider(Start));
		var target = new FakePickerMessageTarget();
		var opened = Open(picker, target);
		var interaction = new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Next)) { ReplacementFailuresRemaining = 1, };

		await picker.HandleAsync(interaction);
		await picker.HandleAsync(Pick(opened.SearchId));

		await Assert.That(interaction.Replacements).HasSingleItem();
		await Assert.That(interaction.Replacements[0].Content).Contains("Something went wrong");
		await Assert.That(target.Operations).IsEmpty();
	}

	[Test]
	public async Task AnUnopenedPickerIsRecordedAsUnavailable()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var logger = new RecordingLogger<MalSearchPicker>();
		var picker = CreatePicker(cache, new ManualTimeProvider(Start), logger);

		await picker.HandleAsync(new FakePickerInteraction(PickerCustomId.Create(SearchId, PickerAction.Next)));

		await Assert.That(EventNames(logger)).IsEquivalentTo(["PickerUnavailable"]);
	}

	[Test]
	public async Task APickThatPostedIsRecordedAsPickedEvenWhenTheEphemeralDeleteFails()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var logger = new RecordingLogger<MalSearchPicker>();
		var picker = CreatePicker(cache, new ManualTimeProvider(Start), logger);
		var target = new FakePickerMessageTarget { DeleteException = new InvalidOperationException("delete failed"), };
		var opened = Open(picker, target);

		await picker.HandleAsync(Pick(opened.SearchId));

		await Assert.That(Events(logger, PickerEndedEvent)).IsEquivalentTo(["Picked"]);
		await Assert.That(EventNames(logger)).Contains("TerminalStatePushFailed");
	}

	[Test]
	public async Task ACancelIsRecordedAsCancelledEvenWhenItsTerminalStateCannotBePushed()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var logger = new RecordingLogger<MalSearchPicker>();
		var picker = CreatePicker(cache, new ManualTimeProvider(Start), logger);
		var opened = Open(picker, new FakePickerMessageTarget());
		var cancel = new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Cancel)) { ReplacementFailuresRemaining = 1, };

		await picker.HandleAsync(cancel);

		await Assert.That(Events(logger, PickerEndedEvent)).IsEquivalentTo(["Cancelled"]);
		await Assert.That(EventNames(logger)).Contains("TerminalStatePushFailed");
	}

	[Test]
	public async Task ARaceBetweenPickAndCancelRecordsOneTerminalEvent()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var logger = new RecordingLogger<MalSearchPicker>();
		var picker = CreatePicker(cache, new ManualTimeProvider(Start), logger);
		var target = new FakePickerMessageTarget { PausePost = true, };
		var opened = Open(picker, target);
		var pick = picker.HandleAsync(Pick(opened.SearchId));
		await target.PostStarted.Task;
		var cancel = picker.HandleAsync(new FakePickerInteraction(PickerCustomId.Create(opened.SearchId, PickerAction.Cancel)));

		target.AllowPost.SetResult();
		await Task.WhenAll(pick, cancel);

		await Assert.That(Events(logger, PickerEndedEvent)).IsEquivalentTo(["Picked"]);
	}

	private static MalSearchPicker CreatePicker(IMemoryCache cache, TimeProvider timeProvider, ILogger<MalSearchPicker>? logger = null) =>
		new(cache, timeProvider, logger ?? NullLogger<MalSearchPicker>.Instance);

	private static IEnumerable<string> EventNames(RecordingLogger<MalSearchPicker> logger) =>
		logger.Entries.Select(static entry => entry.EventId.Name ?? string.Empty);

	private static IEnumerable<string> Events(RecordingLogger<MalSearchPicker> logger, string eventName) =>
		logger.Entries
			  .Where(entry => string.Equals(entry.EventId.Name, eventName, StringComparison.Ordinal))
			  .Select(static entry => entry.State
										   .SingleOrDefault(static field => string.Equals(field.Key, "Outcome", StringComparison.Ordinal))
										   .Value?.ToString() ?? string.Empty);

	private static (Guid SearchId, PickerView View) Open(MalSearchPicker picker, FakePickerMessageTarget target, int resultCount = 2)
	{
		var results = Enumerable.Range(1, resultCount).Select(Result);
		var opening = picker.OpenAsync(SearchId, results, Context(), target);
		if (!opening.IsCompletedSuccessfully)
		{
			throw new InvalidOperationException("The test Picker did not open synchronously.");
		}

		var view = target.Edits.Single();
		target.Operations.Clear();
		target.Edits.Clear();
		return new(SearchId, view);
	}

	private static PickerSearchContext Context() => new(
		Query: "Monster",
		MediaKind: PickerMediaKind.Anime,
		MediaTypeFilter: null,
		DiscordUserId: 1UL,
		RequesterDisplayName: "Requester",
		RequesterAvatarUrl: null,
		GuildId: 2UL,
		ChannelId: 3UL,
		InvokedAt: Start);

	private static FakePickerInteraction Pick(Guid searchId) => new(PickerCustomId.Create(searchId, PickerAction.Pick)) { Values = ["0"], };

	private static SearchResult Result(int id) => new(
		(uint)id,
		$"Result {id.ToString(CultureInfo.InvariantCulture)}",
		MatchRank.Contains,
		static _ => new())
	{
		MediaKind = PickerMediaKind.Anime,
		MediaType = "TV",
		ListUserCount = (uint)id,
	};

	private sealed record FakePickerInteraction(string CustomId) : IPickerInteraction
	{
		public IReadOnlyList<string> Values { get; init; } = [];

		public ulong DiscordUserId { get; init; } = 1UL;

		public string DiscordDisplayName { get; init; } = "Requester";

		public ulong? GuildId { get; init; } = 2UL;

		public ulong? ChannelId { get; init; } = 3UL;

		public int RecognizedCount { get; private set; }

		public int OutcomeFailuresRemaining { get; set; }

		public int ReplacementFailuresRemaining { get; set; }

		public bool PauseReplacement { get; init; }

		public TaskCompletionSource ReplacementStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource AllowReplacement { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public List<PickerInteractionOutcome> Outcomes { get; } = [];

		public List<PickerView> Replacements { get; } = [];

		public async Task ApplyOutcomeAsync(PickerInteractionOutcome outcome, CancellationToken cancellationToken = default)
		{
			if (this.OutcomeFailuresRemaining > 0)
			{
				this.OutcomeFailuresRemaining--;
				throw new InvalidOperationException("outcome failed");
			}

			if (outcome.Replacement is null)
			{
				this.Outcomes.Add(outcome);
				this.RecognizedCount++;
				return;
			}

			if (this.ReplacementFailuresRemaining > 0)
			{
				this.ReplacementFailuresRemaining--;
				throw new InvalidOperationException("replacement failed");
			}

			this.Outcomes.Add(outcome);
			this.Replacements.Add(outcome.Replacement);
			if (this.PauseReplacement)
			{
				this.ReplacementStarted.SetResult();
				await this.AllowReplacement.Task.WaitAsync(cancellationToken);
			}
		}
	}

	private sealed class EvictingMemoryCache : IMemoryCache
	{
		public ICacheEntry CreateEntry(object key) => new Entry(key);

		public void Dispose()
		{
		}

		public void Remove(object key)
		{
		}

		public bool TryGetValue(object key, out object? value)
		{
			value = null;
			return false;
		}

		private sealed class Entry(object _key) : ICacheEntry
		{
			public object Key => _key;

			public object? Value { get; set; }

			public DateTimeOffset? AbsoluteExpiration { get; set; }

			public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

			public TimeSpan? SlidingExpiration { get; set; }

			public IList<IChangeToken> ExpirationTokens { get; } = [];

			public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = [];

			public CacheItemPriority Priority { get; set; }

			public long? Size { get; set; }

			public void Dispose()
			{
				foreach (var registration in this.PostEvictionCallbacks)
				{
					registration.EvictionCallback!(this.Key, this.Value, EvictionReason.Capacity, registration.State);
				}
			}
		}
	}

	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "The task sources are controlled by this test fake")]
	private sealed class FakePickerMessageTarget : IPickerMessageTarget
	{
		private int _editCount;

		public List<string> Operations { get; } = [];

		public List<PickerView> Edits { get; } = [];

		public Exception? PostException { get; init; }

		public Exception? DeleteException { get; init; }

		public Exception? EditException { get; init; }

		public bool PausePost { get; init; }

		public bool PauseDelete { get; init; }

		public int PauseEditNumber { get; init; }

		public int CompletedPostCount { get; private set; }

		public int CompletedEditCount { get; private set; }

		public CancellationToken LastEditCancellationToken { get; private set; }

		public TaskCompletionSource PostStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource AllowPost { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource DeleteStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource AllowDelete { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource EditStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public TaskCompletionSource AllowEdit { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public async Task DeleteOriginalAsync(CancellationToken cancellationToken = default)
		{
			this.Operations.Add(DeleteOperation);
			if (this.DeleteException is not null)
			{
				throw this.DeleteException;
			}

			if (this.PauseDelete)
			{
				this.DeleteStarted.SetResult();
				await this.AllowDelete.Task.WaitAsync(cancellationToken);
			}
		}

		public async Task EditOriginalAsync(PickerView view, CancellationToken cancellationToken = default)
		{
			this.LastEditCancellationToken = cancellationToken;
			this.Operations.Add(EditOperation);
			this.Edits.Add(view);
			this._editCount++;
			if (this.EditException is not null)
			{
				throw this.EditException;
			}

			if (this._editCount == this.PauseEditNumber)
			{
				this.EditStarted.SetResult();
				await this.AllowEdit.Task.WaitAsync(cancellationToken);
			}

			this.CompletedEditCount++;
		}

		public async Task SendPublicAsync(DiscordEmbedBuilder embed, CancellationToken cancellationToken = default)
		{
			this.Operations.Add(PostOperation);
			if (this.PostException is not null)
			{
				throw this.PostException;
			}

			if (this.PausePost)
			{
				this.PostStarted.SetResult();
				await this.AllowPost.Task.WaitAsync(cancellationToken);
			}

			this.CompletedPostCount++;
		}
	}
}
