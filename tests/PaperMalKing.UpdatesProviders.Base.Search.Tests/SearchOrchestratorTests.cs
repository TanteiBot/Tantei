// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace PaperMalKing.UpdatesProviders.Base.Search.Tests;

public sealed class SearchOrchestratorTests
{
	private const string Query = "Monster";
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task AutoPostAwaitsTheAsynchronousBuildAndPostsItsEmbedBeforeDeleting()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var target = new FakeMessageTarget();
		var built = new DiscordEmbedBuilder().WithTitle("Built");
		var provider = new FakeSearchProvider(AutoPost(async (_, _) =>
		{
			await Task.Yield();
			return built;
		}));
		var orchestrator = CreateOrchestrator(cache);

		await orchestrator.RunAsync(provider, new FakeInvocation(target), Request(), CancellationToken.None);

		await Assert.That(target.Operations).IsEquivalentTo(
			[FakeMessageTarget.PostOperation, FakeMessageTarget.DeleteOperation],
			TUnit.Assertions.Enums.CollectionOrdering.Matching);
		await Assert.That(target.Posts.Single()).IsSameReferenceAs(built);
	}

	[Test]
	public async Task AutoPostThatFailsToBuildLeavesOneTerminalErrorAndNoDelete()
	{
		using var cache = new MemoryCache(new MemoryCacheOptions());
		var target = new FakeMessageTarget();
		var provider = new FakeSearchProvider(AutoPost((_, _) => throw new InvalidOperationException("build failed")));
		var orchestrator = CreateOrchestrator(cache);

		await orchestrator.RunAsync(provider, new FakeInvocation(target), Request(), CancellationToken.None);

		await Assert.That(target.Operations).DoesNotContain(FakeMessageTarget.DeleteOperation);
		await Assert.That(target.Edits.Single().Content).IsEqualTo(SearchMessages.PostFailed);
	}

	private static SearchOrchestrator CreateOrchestrator(IMemoryCache cache)
	{
		var time = new FakeTimeProvider(Start);
		var picker = new SearchPicker(cache, time, NullLogger<SearchPicker>.Instance);
		return new(picker, time, NullLogger<SearchOrchestrator>.Instance);
	}

	private static SearchEvaluation AutoPost(Func<PickerSearchContext, CancellationToken, Task<DiscordEmbedBuilder>> buildEmbed)
	{
		var result = new SearchResult(1U, Query, MatchRank.Primary, "TV", buildEmbed);
		return new(SearchOutcomeKind.AutoPosted, [result], FloorSurvivorCount: 1, AutoPostResult: result);
	}

	private static SearchRequest Request() => new(MatchKey.Create(Query), Query, PickerMediaKind.Anime, Filter: null);

	private sealed record FakeSearchProvider(SearchEvaluation Evaluation) : IMediaSearchProvider
	{
		public SearchProviderIdentity Identity { get; } = new("Fake", "fake");

		public int MinimumQueryLength => 1;

		public Task<SearchEvaluation> EvaluateAsync(SearchRequest request, CancellationToken cancellationToken) => Task.FromResult(this.Evaluation);

		public SearchFailure Classify(Exception exception) => new(SearchMessages.Failed(this.Identity.DisplayName), static _ => { });
	}

	private sealed record FakeInvocation(IPickerMessageTarget Target) : ISearchInvocation
	{
		public bool CanPostEmbed => true;

		public bool IncludeNsfw => false;

		public ulong DiscordUserId => 1UL;

		public string RequesterDisplayName => "Requester";

		public string? RequesterAvatarUrl => null;

		public ulong GuildId { get; } = 2UL;

		public ulong ChannelId { get; } = 3UL;
	}

	private sealed class FakeMessageTarget : IPickerMessageTarget
	{
		public const string PostOperation = "post";
		public const string DeleteOperation = "delete";
		public const string EditOperation = "edit";

		public List<string> Operations { get; } = [];

		public List<PickerView> Edits { get; } = [];

		public List<DiscordEmbedBuilder> Posts { get; } = [];

		public Task SendPublicAsync(DiscordEmbedBuilder embed, CancellationToken cancellationToken = default)
		{
			this.Operations.Add(PostOperation);
			this.Posts.Add(embed);
			return Task.CompletedTask;
		}

		public Task DeleteOriginalAsync(CancellationToken cancellationToken = default)
		{
			this.Operations.Add(DeleteOperation);
			return Task.CompletedTask;
		}

		public Task EditOriginalAsync(PickerView view, CancellationToken cancellationToken = default)
		{
			this.Operations.Add(EditOperation);
			this.Edits.Add(view);
			return Task.CompletedTask;
		}
	}
}
