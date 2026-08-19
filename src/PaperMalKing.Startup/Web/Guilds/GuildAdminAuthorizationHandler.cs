// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PaperMalKing.Startup.Web.Guilds;

public sealed class GuildAdminAuthorizationHandler(IBotGuildPresence _botGuildPresence) : AuthorizationHandler<GuildAdminRequirement, ulong>
{
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GuildAdminRequirement requirement, ulong resource)
	{
		if (string.Equals(context.User.FindFirstValue(TanteiClaimTypes.WebAdmin), "true", StringComparison.Ordinal))
		{
			context.Succeed(requirement);
			return;
		}

		if (!ulong.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var discordUserId))
		{
			return;
		}

		if (await _botGuildPresence.IsGuildAdminAsync(resource, discordUserId))
		{
			context.Succeed(requirement);
		}
	}
}
