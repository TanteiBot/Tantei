// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Security.Claims;
using DSharpPlus;
using Microsoft.Extensions.Options;
using PaperMalKing.Startup.Web;
using PaperMalKing.Startup.Web.Guilds;

namespace Tantei.Tests;

public sealed class InviteAuthorizationTests
{
	private const ulong UserId = 42UL;

	private const ulong SharedGuildId = 100UL;

	private const ulong OtherGuildId = 200UL;

	private sealed class FakeBotGuildPresence(params ulong[] presentGuildIds) : IBotGuildPresence
	{
		public BotGuildInfo? GetGuild(ulong guildId)
			=> presentGuildIds.Contains(guildId) ? new(guildId, $"Guild {guildId}", IconUrl: null) : null;

		public Task<bool> IsGuildAdminAsync(ulong guildId, ulong discordUserId) => Task.FromResult(false);
	}

	private sealed class FakeUserGuildsProvider(IReadOnlyList<DiscordPartialGuild>? guilds) : IUserGuildsProvider
	{
		public Task<IReadOnlyList<DiscordPartialGuild>?> GetGuildsAsync(ulong discordUserId, CancellationToken cancellationToken)
			=> Task.FromResult(guilds);
	}

	private static ClaimsPrincipal CreateUser(bool isWebAdmin = false)
	{
		var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, UserId.ToString(System.Globalization.CultureInfo.InvariantCulture)), };
		if (isWebAdmin)
		{
			claims.Add(new(TanteiClaimTypes.WebAdmin, "true"));
		}

		return new(new ClaimsIdentity(claims, "Test"));
	}

	private static InviteAuthorization CreateAuthorization(InviteMode mode, IReadOnlyList<DiscordPartialGuild>? guilds,
														   IReadOnlyList<ulong>? presentGuildIds = null)
		=> new(Options.Create(new WebOptions { InviteMode = mode, }),
			new FakeBotGuildPresence([.. presentGuildIds ?? []]),
			new FakeUserGuildsProvider(guilds));

	[Test]
	[Arguments(InviteMode.Private)]
	[Arguments(InviteMode.SemiPrivate)]
	[Arguments(InviteMode.Public)]
	public async Task WebAdminIsAllowedInEveryMode(InviteMode mode)
	{
		var authorization = CreateAuthorization(mode: mode, guilds: null);

		var eligibility = await authorization.GetEligibilityAsync(CreateUser(isWebAdmin: true), TestContext.Current!.Execution.CancellationToken);

		await Assert.That(eligibility).IsEqualTo(InviteEligibility.Allowed);
	}

	[Test]
	public async Task PrivateDeniesEveryoneElse()
	{
		var authorization = CreateAuthorization(mode: InviteMode.Private, guilds: []);

		var eligibility = await authorization.GetEligibilityAsync(CreateUser(), TestContext.Current!.Execution.CancellationToken);

		await Assert.That(eligibility).IsEqualTo(InviteEligibility.NotAllowed);
	}

	[Test]
	public async Task PublicAllowsEveryoneWithoutConsultingGuilds()
	{
		var authorization = CreateAuthorization(mode: InviteMode.Public, guilds: null);

		var eligibility = await authorization.GetEligibilityAsync(CreateUser(), TestContext.Current!.Execution.CancellationToken);

		await Assert.That(eligibility).IsEqualTo(InviteEligibility.Allowed);
	}

	[Test]
	public async Task SemiPrivateAllowsSomeoneSharingAGuildWithTheBot()
	{
		var authorization = CreateAuthorization(mode: InviteMode.SemiPrivate,
			guilds: [new(SharedGuildId, "Shared", IconUrl: null, Permissions.None),],
			presentGuildIds: [SharedGuildId,]);

		var eligibility = await authorization.GetEligibilityAsync(CreateUser(), TestContext.Current!.Execution.CancellationToken);

		await Assert.That(eligibility).IsEqualTo(InviteEligibility.Allowed);
	}

	[Test]
	public async Task SemiPrivateDeniesSomeoneSharingNoGuildWithTheBot()
	{
		var authorization = CreateAuthorization(mode: InviteMode.SemiPrivate,
			guilds: [new(OtherGuildId, "Elsewhere", IconUrl: null, Permissions.ManageGuild),],
			presentGuildIds: [SharedGuildId,]);

		var eligibility = await authorization.GetEligibilityAsync(CreateUser(), TestContext.Current!.Execution.CancellationToken);

		await Assert.That(eligibility).IsEqualTo(InviteEligibility.NotAllowed);
	}

	[Test]
	public async Task SemiPrivateReportsUnknownRatherThanDenyingWhenGuildsCannotBeResolved()
	{
		var authorization = CreateAuthorization(mode: InviteMode.SemiPrivate, guilds: null, presentGuildIds: [SharedGuildId,]);

		var eligibility = await authorization.GetEligibilityAsync(CreateUser(), TestContext.Current!.Execution.CancellationToken);

		await Assert.That(eligibility).IsEqualTo(InviteEligibility.Unknown);
	}
}
