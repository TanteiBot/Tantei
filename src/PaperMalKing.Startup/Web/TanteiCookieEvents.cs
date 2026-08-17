// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;

namespace PaperMalKing.Startup.Web;

public sealed class TanteiCookieEvents(IDbContextFactory<DatabaseContext> _dbContextFactory,
									   ApplicationOwnersProvider _applicationOwnersProvider,
									   ILogger<TanteiCookieEvents> _logger) : CookieAuthenticationEvents
{
	public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
	{
		var principal = context.Principal;
		var nameIdentifier = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
		if (principal is null || !ulong.TryParse(nameIdentifier, out var discordUserId))
		{
			_logger.RejectingPrincipalWithoutDiscordId();
			context.RejectPrincipal();
			return;
		}

		bool isRegistered;
		await using (var db = await _dbContextFactory.CreateDbContextAsync(context.HttpContext.RequestAborted))
		{
			isRegistered = db.GetDiscordUserById(discordUserId) is not null;
		}

		var isWebAdmin = await _applicationOwnersProvider.IsOwnerAsync(discordUserId, context.HttpContext.RequestAborted);

		var identity = new ClaimsIdentity();
		ReplaceClaim(principal, identity, TanteiClaimTypes.Registered, isRegistered);
		ReplaceClaim(principal, identity, TanteiClaimTypes.WebAdmin, isWebAdmin);
		principal.AddIdentity(identity);
	}

	private static void ReplaceClaim(ClaimsPrincipal principal, ClaimsIdentity identity, string claimType, bool value)
	{
		foreach (var existing in principal.FindAll(claimType).ToArray())
		{
			existing.Subject?.RemoveClaim(existing);
		}

		identity.AddClaim(new(claimType, value ? "true" : "false"));
	}
}
