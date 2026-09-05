// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using DSharpPlus;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

	private sealed class FakeBotGuildPresence(params ulong[] presentGuildIds) : IBotGuildPresence
	{
		public BotGuildInfo? GetGuild(ulong guildId)
			=> presentGuildIds.Contains(guildId) ? new(guildId, $"Guild {guildId}", IconUrl: null) : null;

		public Task<bool> IsGuildAdminAsync(ulong guildId, ulong discordUserId) => Task.FromResult(false);
	}

	private static async Task<(IDbContextFactory<DatabaseContext> Factory, SqliteConnection Connection)> CreateDatabaseAsync()
	{
		var (factory, connection, _) = await SqliteInMemoryDatabase.CreateAsync();
		return (factory, connection);
	}

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	private static void SeedUserInGuilds(IDbContextFactory<DatabaseContext> factory, ulong discordUserId, params ulong[] guildIds)
	{
		using var db = factory.CreateDbContext();
		var guilds = guildIds.Select(id => new DiscordGuild { DiscordGuildId = id, PostingChannelId = id, Users = [], }).ToList();
		db.DiscordGuilds.AddRange(guilds);
		db.DiscordUsers.Add(new()
		{
			DiscordUserId = discordUserId,
			BotUser = new(),
			Guilds = guilds,
		});
		db.SaveChanges();
	}

	[Test]
	public async Task ReturnsGuildsTheUserIsRegisteredInAndTheBotIsPresentIn()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		SeedUserInGuilds(factory, FirstUserId, FirstGuildId, SecondGuildId);
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId, SecondGuildId));

		var result = service.GetManageableGuilds(FirstUserId);

		await Assert.That(result.Select(g => g.GuildId).Order()).IsEquivalentTo([FirstGuildId, SecondGuildId,]);
	}

	[Test]
	public async Task SkipsGuildsTheBotIsNoLongerPresentIn()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		SeedUserInGuilds(factory, FirstUserId, FirstGuildId, SecondGuildId);
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId));

		var result = service.GetManageableGuilds(FirstUserId);

		await Assert.That(result.Select(g => g.GuildId)).IsEquivalentTo([FirstGuildId,]);
	}

	[Test]
	public async Task ReturnsEmptyForAnUnknownUser()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId));

		var result = service.GetManageableGuilds(UnknownUserId);

		await Assert.That(result).IsEmpty();
	}

	[Test]
	public async Task InvitableGuildsAreThoseWithManageGuildWhereTheBotIsAbsent()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId));

		var result = service.GetInvitableGuilds([
			new(FirstGuildId, "Bot is here", null, Permissions.ManageGuild),
			new(ThirdGuildId, "Can invite", null, Permissions.ManageGuild),
			new(FourthGuildId, "No permission", null, Permissions.None),
		]);

		await Assert.That(result.Select(g => g.GuildId)).IsEquivalentTo([ThirdGuildId,]);
	}

	[Test]
	public async Task InvitableGuildsAreEmptyWhenTheUserAdministersNothingNew()
	{
		var (factory, connection) = await CreateDatabaseAsync();
		await using var ownedConnection = connection;
		var service = new GuildQueryService(factory, new FakeBotGuildPresence(FirstGuildId));

		await Assert.That(service.GetInvitableGuilds([new(FirstGuildId, "Bot is here", null, Permissions.ManageGuild),])).IsEmpty();
	}
}
