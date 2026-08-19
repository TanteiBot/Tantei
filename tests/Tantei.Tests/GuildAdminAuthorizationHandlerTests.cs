// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PaperMalKing.Startup.Web;
using PaperMalKing.Startup.Web.Guilds;

namespace Tantei.Tests;

public sealed class GuildAdminAuthorizationHandlerTests
{
	private const ulong GuildId = 42UL;

	private const ulong DiscordUserId = 100UL;

	private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal user) =>
		new([new GuildAdminRequirement()], user, GuildId);

	private static ClaimsPrincipal CreatePrincipal(string? webAdminClaimValue)
	{
		var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, DiscordUserId.ToString(System.Globalization.CultureInfo.InvariantCulture)), };
		if (webAdminClaimValue is not null)
		{
			claims.Add(new(TanteiClaimTypes.WebAdmin, webAdminClaimValue));
		}

		return new(new ClaimsIdentity(claims, "test"));
	}

	[Test]
	[Arguments("true")]
	public async Task ExactStringTrueBypassesGuildCheck(string claimValue)
	{
		var botGuildPresence = new FakeBotGuildPresence(isGuildAdmin: false);
		var handler = new GuildAdminAuthorizationHandler(botGuildPresence);
		var context = CreateContext(CreatePrincipal(claimValue));

		await handler.HandleAsync(context);

		await Assert.That(context.HasSucceeded).IsTrue();
		await Assert.That(botGuildPresence.CallCount).IsEqualTo(0);
	}

	[Test]
	[Arguments("True")]
	[Arguments("TRUE")]
	[Arguments("1")]
	[Arguments("yes")]
	public async Task NonExactWebAdminClaimValuesDoNotBypassGuildCheck(string claimValue)
	{
		var botGuildPresence = new FakeBotGuildPresence(isGuildAdmin: false);
		var handler = new GuildAdminAuthorizationHandler(botGuildPresence);
		var context = CreateContext(CreatePrincipal(claimValue));

		await handler.HandleAsync(context);

		await Assert.That(context.HasSucceeded).IsFalse();
		await Assert.That(botGuildPresence.CallCount).IsEqualTo(1);
	}

	[Test]
	public async Task MissingWebAdminClaimFallsThroughToGuildCheck()
	{
		var botGuildPresence = new FakeBotGuildPresence(isGuildAdmin: true);
		var handler = new GuildAdminAuthorizationHandler(botGuildPresence);
		var context = CreateContext(CreatePrincipal(webAdminClaimValue: null));

		await handler.HandleAsync(context);

		await Assert.That(context.HasSucceeded).IsTrue();
		await Assert.That(botGuildPresence.CallCount).IsEqualTo(1);
	}

	[Test]
	public async Task NonAdminNonOwnerIsNotSucceeded()
	{
		var botGuildPresence = new FakeBotGuildPresence(isGuildAdmin: false);
		var handler = new GuildAdminAuthorizationHandler(botGuildPresence);
		var context = CreateContext(CreatePrincipal(webAdminClaimValue: null));

		await handler.HandleAsync(context);

		await Assert.That(context.HasSucceeded).IsFalse();
	}

	private sealed class FakeBotGuildPresence(bool isGuildAdmin) : IBotGuildPresence
	{
		public int CallCount { get; private set; }

		public BotGuildInfo? GetGuild(ulong guildId) => throw new NotSupportedException();

		public Task<bool> IsGuildAdminAsync(ulong guildId, ulong discordUserId)
		{
			this.CallCount++;
			return Task.FromResult(isGuildAdmin);
		}
	}
}
