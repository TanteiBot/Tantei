// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests;

public sealed class MalUpdateProviderTests
{
	[Test]
	[Arguments(MalUserFeatures.AnimeList)]
	[Arguments(MalUserFeatures.MangaList)]
	public async Task OfficialApiForbiddenEndsThePollingCycle(MalUserFeatures feature)
	{
		await using var scope = await ProviderScope.CreateAsync(new HttpRequestException("forbidden", inner: null, HttpStatusCode.Forbidden), feature);

		await scope.Provider.CheckForUpdatesOnceAsync(CancellationToken.None);

		await Assert.That(scope.Client.ProfileCalls).Count().IsEqualTo(1);
		await Assert.That(scope.Client.ListCallCount).IsEqualTo(1);
		await Assert.That(EventNames(scope.Logger)).Contains("OfficialApiForbiddenDuringUpdateCheck");
		await Assert.That(EventNames(scope.Logger)).DoesNotContain("ErrorWhileCheckingUpdatesForUser");
	}

	[Test]
	public async Task UnofficialPrivateProfileIsNotReportedAsAnOfficialApiFailure()
	{
		await using var scope = await ProviderScope.CreateAsync(listException: null,
			profileException: new HttpRequestException("private", inner: null, HttpStatusCode.Forbidden));

		await scope.Provider.CheckForUpdatesOnceAsync(CancellationToken.None);

		await Assert.That(scope.Client.ProfileCalls).Count().IsEqualTo(1);
		await Assert.That(scope.Client.ListCallCount).IsEqualTo(0);
		await Assert.That(EventNames(scope.Logger)).DoesNotContain("OfficialApiForbiddenDuringUpdateCheck");
	}

	private static IEnumerable<string?> EventNames(RecordingLogger<MalUpdateProvider> logger) =>
		logger.Entries.Select(static entry => entry.EventId.Name);

	private sealed class ProviderScope : IAsyncDisposable
	{
		private readonly SqliteConnection _connection;

		public FakeMyAnimeListClient Client { get; }

		public RecordingLogger<MalUpdateProvider> Logger { get; } = new();

		public MalUpdateProvider Provider { get; }

		private ProviderScope(SqliteConnection connection, FakeMyAnimeListClient client, IDbContextFactory<DatabaseContext> factory)
		{
			this._connection = connection;
			this.Client = client;
			this.Provider = new(this.Logger, new StaticOptionsMonitor<MalOptions>(new() { DelayBetweenChecksInMilliseconds = 1, }), client, factory);
			this.Provider.UpdateFoundEvent += static (_, _) => Task.CompletedTask;
		}

		public static async Task<ProviderScope> CreateAsync(Exception? listException, MalUserFeatures feature = MalUserFeatures.AnimeList, Exception? profileException = null)
		{
			const int userCount = 2;
			var connection = new SqliteConnection("Filename=:memory:");
			await connection.OpenAsync();
			var options = new DbContextOptionsBuilder<DatabaseContext>().UseSqlite(connection).Options;
			var factory = new TestDbContextFactory(options);
			await using (var db = factory.CreateDbContext())
			{
				await db.Database.EnsureCreatedAsync();
				SeedUsers(db, userCount, feature);
				db.SaveChanges();
			}

			return new(connection, new() { ListException = listException, ProfileException = profileException, }, factory);
		}

		public async ValueTask DisposeAsync()
		{
			this.Provider.Dispose();
			await this._connection.DisposeAsync();
		}

		private static void SeedUsers(DatabaseContext db, int count, MalUserFeatures feature)
		{
			for (uint id = 1; id <= count; id++)
			{
				var guildUsers = new List<DiscordUser>();
				var guild = new DiscordGuild { DiscordGuildId = id, PostingChannelId = id, Users = guildUsers, };
				var discordUser = new DiscordUser { DiscordUserId = id, BotUser = new(), Guilds = [guild,], };
				guildUsers.Add(discordUser);
				db.MalUsers.Add(new()
				{
					UserId = id,
					DiscordUserId = id,
					DiscordUser = discordUser,
					Username = $"user-{id}",
					Features = feature,
					LastUpdatedAnimeListTimestamp = DateTimeOffset.UtcNow,
					LastUpdatedMangaListTimestamp = DateTimeOffset.UtcNow,
					LastAnimeUpdateHash = "old",
					LastMangaUpdateHash = "old",
					FavoritesIdHash = string.Empty,
					Colors = [],
				});
			}
		}
	}

	private sealed class FakeMyAnimeListClient : IMyAnimeListClient
	{
		public Exception? ListException { get; init; }

		public Exception? ProfileException { get; init; }

		public List<string> ProfileCalls { get; } = [];

		public int ListCallCount { get; private set; }

		public Task<User> GetUserAsync(string username, ParserOptions options, CancellationToken cancellationToken)
		{
			this.ProfileCalls.Add(username);
			return this.ProfileException is null
				? Task.FromResult(new User { Id = 1, Username = username, LatestAnimeUpdateHash = "new", LatestMangaUpdateHash = "new", Favorites = UserFavorites.Empty, })
				: Task.FromException<User>(this.ProfileException);
		}

		public Task<IReadOnlyList<TE>> GetLatestListUpdatesAsync<TE, TListType, TRequestOptions, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
			string username, TRequestOptions requestOptions, CancellationToken cancellationToken)
			where TE : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
			where TListType : IListType
			where TRequestOptions : unmanaged, Enum
			where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
			where TStatus : BaseListEntryStatus<TListStatus>
			where TMediaType : unmanaged, Enum
			where TNodeStatus : unmanaged, Enum
			where TListStatus : unmanaged, Enum
		{
			this.ListCallCount++;
			return this.ListException is null
				? Task.FromResult<IReadOnlyList<TE>>([])
				: Task.FromException<IReadOnlyList<TE>>(this.ListException);
		}

		public Task<string> GetUsernameAsync(uint id, CancellationToken cancellationToken) => throw new NotSupportedException();

		public Task<IReadOnlyList<AnimeSearchResult>> SearchAnimeAsync(string query, bool includeNsfw, CancellationToken cancellationToken) =>
			throw new NotSupportedException();

		public Task<IReadOnlyList<MangaSearchResult>> SearchMangaAsync(string query, bool includeNsfw, CancellationToken cancellationToken) =>
			throw new NotSupportedException();

		public Task<MediaInfo> GetAnimeDetailsAsync(long id, CancellationToken cancellationToken) => throw new NotSupportedException();

		public Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken) => throw new NotSupportedException();

		public Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken) => throw new NotSupportedException();
	}

	private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
	{
		public T CurrentValue => value;

		public T Get(string? name) => value;

		public IDisposable? OnChange(Action<T, string?> listener) => null;
	}

	private sealed class TestDbContextFactory(DbContextOptions<DatabaseContext> options) : IDbContextFactory<DatabaseContext>
	{
		public DatabaseContext CreateDbContext() => new(options);
	}
}
