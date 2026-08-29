// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Security.Claims;
using Microsoft.Extensions.Options;
using PaperMalKing.Startup.Web.Guilds;

namespace PaperMalKing.Startup.Web;

public sealed class InviteAuthorization(IOptions<WebOptions> _webOptions, IBotGuildPresence _botGuildPresence, IUserGuildsProvider _userGuildsProvider)
{
	public InviteMode Mode => _webOptions.Value.InviteMode;

	public async Task<InviteEligibility> GetEligibilityAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
	{
		if (string.Equals(user.FindFirstValue(TanteiClaimTypes.WebAdmin), "true", StringComparison.Ordinal))
		{
			return InviteEligibility.Allowed;
		}

		switch (this.Mode)
		{
			case InviteMode.Public:
				return InviteEligibility.Allowed;
			case InviteMode.SemiPrivate:
				if (!ulong.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var discordUserId))
				{
					return InviteEligibility.NotAllowed;
				}

				var guilds = await _userGuildsProvider.GetGuildsAsync(discordUserId, cancellationToken);
				if (guilds is null)
				{
					return InviteEligibility.Unknown;
				}

				return guilds.Any(guild => _botGuildPresence.GetGuild(guild.Id) is not null)
					? InviteEligibility.Allowed
					: InviteEligibility.NotAllowed;
			default:
				return InviteEligibility.NotAllowed;
		}
	}
}
