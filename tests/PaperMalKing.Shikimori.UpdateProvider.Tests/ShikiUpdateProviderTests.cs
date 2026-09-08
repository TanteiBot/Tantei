// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.UpdateProvider.Achievements;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests;

public sealed class ShikiUpdateProviderTests
{
	private const uint CharacterId = 62;

	private const string BestKnownWorkName = "Best Known Show";

	private const string BestKnownWorkUrl = "https://shikimori.one/animes/1";

	[Test]
	public async Task AChangedFavouritePostsWhatTheEnrichmentModuleReturned()
	{
		var client = new FakeShikiClient
		{
			Favourites = new() { Characters = [new() { Id = CharacterId, Name = "Stored Character", },], },
			Favourite = new()
			{
				FavouritesInfo = new()
				{
					Characters =
					[
						new()
						{
							Id = CharacterId,
							Name = "Fav Character",
							Url = $"https://shikimori.one/characters/{CharacterId}",
						},
					],
				},
				CharacterDetails = new()
				{
					Animes = [new() { Id = 1, Name = BestKnownWorkName, Score = 8.5f, Url = BestKnownWorkUrl, },],
				},
			},
		};
		await using var scope = await ProviderScope.CreateAsync(client);

		var updates = await scope.CollectUpdatesAsync();

		var embed = updates.Single().EmbedBuilder;
		await Assert.That(embed.Title).IsEqualTo("Fav Character [Character]");
		await Assert.That(embed.Fields.Single(static field => string.Equals(field.Name, "From", StringComparison.Ordinal)).Value)
					.IsEqualTo($"[{BestKnownWorkName}]({BestKnownWorkUrl})");
	}

	private sealed class ProviderScope : IAsyncDisposable
	{
		private readonly SqliteConnection _connection;
		private readonly List<UpdateContents> _updates = [];

		private RecordingLogger<ShikiUpdateProvider> Logger { get; } = new();

		private ShikiUpdateProvider Provider { get; }

		private ProviderScope(SqliteConnection connection, FakeShikiClient client, IDbContextFactory<DatabaseContext> factory)
		{
			this._connection = connection;
			this.Provider = new(this.Logger, new StaticOptionsMonitor<ShikiOptions>(new() { DelayBetweenChecksInMilliseconds = 1, }), client, factory,
				new(new(), NullLogger<ShikiAchievementsService>.Instance));
			this.Provider.UpdateFoundEvent += async (_, args) =>
			{
				await foreach (var update in args.Update.GetUpdateContentsAsync())
				{
					this._updates.Add(update);
				}
			};
		}

		public static async Task<ProviderScope> CreateAsync(FakeShikiClient client)
		{
			var connection = new SqliteConnection("Filename=:memory:");
			await connection.OpenAsync();
			var options = new DbContextOptionsBuilder<DatabaseContext>().UseSqlite(connection).Options;
			var factory = new TestDbContextFactory(options);
			await using (var db = factory.CreateDbContext())
			{
				await db.Database.EnsureCreatedAsync();
				var guildUsers = new List<DiscordUser>();
				var guild = new DiscordGuild { DiscordGuildId = 1, PostingChannelId = 1, Users = guildUsers, };
				var discordUser = new DiscordUser { DiscordUserId = 1, BotUser = new(), Guilds = [guild,], };
				guildUsers.Add(discordUser);
				db.ShikiUsers.Add(new()
				{
					Id = 1,
					DiscordUserId = 1,
					DiscordUser = discordUser,
					Features = ShikiUserFeatures.Default,
					FavouritesIdHash = string.Empty,
					Favourites = [],
					Achievements = [],
					Colors = [],
				});
				db.SaveChanges();
			}

			return new(connection, client, factory);
		}

		public async Task<IReadOnlyList<UpdateContents>> CollectUpdatesAsync()
		{
			await this.Provider.CheckForUpdatesOnceAsync(CancellationToken.None);

			await Assert.That(this.Logger.Entries.Select(static entry => entry.EventId.Name)).DoesNotContain("ErrorWhileCheckingUpdatesForUser");

			return this._updates;
		}

		public async ValueTask DisposeAsync()
		{
			this.Provider.Dispose();
			await this._connection.DisposeAsync();
		}
	}
}
