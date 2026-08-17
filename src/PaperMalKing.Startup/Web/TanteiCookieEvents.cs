// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;

namespace PaperMalKing.Startup.Web;

public sealed class TanteiCookieEvents(IDbContextFactory<DatabaseContext> _dbContextFactory,
									   IApplicationOwners _applicationOwners,
									   ILogger<TanteiCookieEvents> _logger) : CookieAuthenticationEvents
{
	private const string ApiPathPrefix = "/api";

	public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
	{
		if (context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase))
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return Task.CompletedTask;
		}

		return base.RedirectToLogin(context);
	}

	public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
	{
		if (context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase))
		{
			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			return Task.CompletedTask;
		}

		return base.RedirectToAccessDenied(context);
	}

	public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
	{
		var principal = context.Principal;
		var nameIdentifier = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
		if (principal is null || !ulong.TryParse(nameIdentifier, out var discordUserId))
		{
			_logger.RejectingPrincipalWithoutDiscordId();
			context.RejectPrincipal();
			await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return;
		}

		bool isRegistered;
		await using (var db = await _dbContextFactory.CreateDbContextAsync(context.HttpContext.RequestAborted))
		{
			isRegistered = db.DiscordUserExists(discordUserId);
		}

		var isWebAdmin = _applicationOwners.IsOwner(discordUserId);

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
