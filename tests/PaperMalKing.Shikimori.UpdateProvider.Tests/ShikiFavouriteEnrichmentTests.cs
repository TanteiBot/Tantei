// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.UpdateProvider.Achievements;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests;

public sealed class ShikiFavouriteEnrichmentTests
{
	private const uint CharacterId = 62;

	private const int MediaFavouriteCount = 5;

	[Test]
	public async Task EveryChangedMediaFavouriteOfATickRidesOneCallForFavouriteInfo()
	{
		var animes = Enumerable.Range(1, MediaFavouriteCount).Select(static i => Entry((uint)i, string.Create(CultureInfo.InvariantCulture, $"Anime {i}"))).ToArray();
		var client = new FakeShikiClient
		{
			Favourites = new() { Animes = animes, },
			FavouritesInfo = new() { Animes = [.. animes.Select(static entry => AnimeMedia(entry.Id))], },
		};
		await using var scope = await ProviderScope.CreateAsync(client);

		var updates = await scope.CollectUpdatesAsync();

		await Assert.That(client.FavouritesInfoCalls).Count().IsEqualTo(1);
		await Assert.That(client.FavouritesInfoCalls[0].AnimeIds).IsEquivalentTo(animes.Select(static entry => entry.Id));
		await Assert.That(updates).Count().IsEqualTo(MediaFavouriteCount);
	}

	[Test]
	public async Task AThrownCharacterDetailsFetchDropsOnlyItsFieldAndStillPosts()
	{
		var client = new FakeShikiClient
		{
			Favourites = new() { Characters = [Entry(CharacterId, "Stored Character"),], },
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
			CharacterDetailsException = new HttpRequestException("boom"),
		};
		await using var scope = await ProviderScope.CreateAsync(client);

		var updates = await scope.CollectUpdatesAsync();

		await Assert.That(client.CharacterDetailsCalls).IsEquivalentTo([CharacterId,]);
		var embed = updates.Single().EmbedBuilder;
		await Assert.That(embed.Title).IsEqualTo("Fav Character [Character]");
		await Assert.That(embed.Fields.Select(static field => field.Name)).DoesNotContain("From");
		await Assert.That(scope.Logger.Entries.Select(static entry => entry.EventId.Name)).Contains("FailedToEnrichFavourite");
	}

	private static FavouriteEntry Entry(uint id, string name) => new() { Id = id, Name = name, };

	private static AnimeMedia AnimeMedia(uint id) => new()
	{
		Id = id,
		Name = $"Canonical Anime {id}",
		Kind = "tv",
		Status = "released",
		Url = $"https://shikimori.one/animes/{id}",
	};

	private sealed class ProviderScope : IAsyncDisposable
	{
		private readonly SqliteConnection _connection;
		private readonly List<UpdateContents> _updates = [];

		public RecordingLogger<ShikiUpdateProvider> Logger { get; } = new();

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

	private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
	{
		public T CurrentValue => value;

		public T Get(string? name) => value;

		public IDisposable? OnChange(Action<T, string?> listener) => null;
	}
}
