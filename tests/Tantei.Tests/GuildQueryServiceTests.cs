// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Startup.Web.Guilds;

namespace Tantei.Tests;

public sealed class GuildQueryServiceTests
{
	private const ulong FirstUserId = 42UL;

	private const ulong UnknownUserId = 999UL;

	private const ulong FirstGuildId = 100UL;

	private const ulong SecondGuildId = 200UL;

	private const ulong ThirdGuildId = 300UL;

	private const ulong FourthGuildId = 400UL;

	private const ulong ManageGuildPermission = 0x20UL;

	private sealed class FakeBotGuildPresence(params ulong[] presentGuildIds) : IBotGuildPresence
	{
		public BotGuildInfo? GetGuild(ulong guildId)
			=> presentGuildIds.Contains(guildId) ? new(guildId, $"Guild {guildId}", IconUrl: null) : null;

		public Task<bool> IsGuildAdminAsync(ulong guildId, ulong discordUserId) => Task.FromResult(false);
	}

	private static async Task<(IDbContextFactory<DatabaseContext> Factory, SqliteConnection Connection)> CreateDatabaseAsync()
	{
		var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync();
		var services = new ServiceCollection();
		services.AddDbContextFactory<DatabaseContext>(o => o.UseSqlite(connection));
		var provider = services.BuildServiceProvider();
		var factory = provider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
		await using (var db = await factory.CreateDbContextAsync())
		{
			await db.Database.EnsureCreatedAsync();
		}

		return (factory, connection);
	}

	private static async Task SeedUserInGuildsAsync(IDbContextFactory<DatabaseContext> factory, ulong discordUserId, params ulong[] guildIds)
	{
		await using var db = await factory.CreateDbContextAsync();
		var guilds = guildIds.Select(id => new DiscordGuild { DiscordGuildId = id, PostingChannelId = id, Users = [], }).ToList();
		db.DiscordGuilds.AddRange(guilds);
		db.DiscordUsers.Add(new()
		{
			DiscordUserId = discordUserId,
			BotUser = new(),
			Guilds = guilds,
		});
		await db.SaveChangesAsync();
	}

	[Test]
	public async Task ReturnsGuildsTheUserIsRegisteredInAndTheBotIsPresentIn()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		await SeedUserInGuildsAsync(factory, FirstUserId, FirstGuildId, SecondGuildId);
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId, SecondGuildId), new(memoryCache));

		var result = await service.GetManageableGuildsAsync(FirstUserId, cancellationToken);

		await Assert.That(result.Select(g => g.GuildId).Order()).IsEquivalentTo([FirstGuildId, SecondGuildId,]);
	}

	[Test]
	public async Task SkipsGuildsTheBotIsNoLongerPresentIn()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		await SeedUserInGuildsAsync(factory, FirstUserId, FirstGuildId, SecondGuildId);
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId), new(memoryCache));

		var result = await service.GetManageableGuildsAsync(FirstUserId, cancellationToken);

		await Assert.That(result.Select(g => g.GuildId)).IsEquivalentTo([FirstGuildId,]);
	}

	[Test]
	public async Task ReturnsEmptyForAnUnknownUser()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId), new(memoryCache));

		var result = await service.GetManageableGuildsAsync(UnknownUserId, cancellationToken);

		await Assert.That(result).IsEmpty();
	}

	[Test]
	public async Task InvitableGuildsAreThoseWithManageGuildWhereTheBotIsAbsent()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var cache = new UserGuildsCache(memoryCache);
		cache.Set(FirstUserId,
			[
				new(FirstGuildId, "Bot is here", null, ManageGuildPermission),
				new(ThirdGuildId, "Can invite", null, ManageGuildPermission),
				new(FourthGuildId, "No permission", null, 0UL),
			]);
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId), cache);

		var result = service.GetInvitableGuilds(FirstUserId);

		await Assert.That(result.Select(g => g.GuildId)).IsEquivalentTo([ThirdGuildId,]);
	}

	[Test]
	public async Task InvitableGuildsAreEmptyWhenNothingIsCached()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var emptyCache = new UserGuildsCache(memoryCache);
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId), emptyCache);

		await Assert.That(service.GetInvitableGuilds(FirstUserId)).IsEmpty();
	}
}
