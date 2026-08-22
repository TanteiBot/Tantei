// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
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
									   IProblemDetailsService _problemDetailsService,
									   ILogger<TanteiCookieEvents> _logger) : CookieAuthenticationEvents
{
	private const string ApiPathPrefix = "/api";

	public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
	{
		if (context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase))
		{
			return this.WriteProblemAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Unauthorized",
										  "Authentication is required to access this resource.");
		}

		return base.RedirectToLogin(context);
	}

	public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
	{
		if (context.Request.Path.StartsWithSegments(ApiPathPrefix, StringComparison.OrdinalIgnoreCase))
		{
			return this.WriteProblemAsync(context.HttpContext, StatusCodes.Status403Forbidden, "Forbidden",
										  "The authenticated user is not allowed to access this resource.");
		}

		return base.RedirectToAccessDenied(context);
	}

	private Task WriteProblemAsync(HttpContext httpContext, int statusCode, string title, string detail)
	{
		httpContext.Response.StatusCode = statusCode;
		return _problemDetailsService.WriteAsync(new()
		{
			HttpContext = httpContext,
			ProblemDetails =
			{
				Status = statusCode,
				Title = title,
				Detail = detail,
			},
		}).AsTask();
	}

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
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
		using (var db = _dbContextFactory.CreateDbContext())
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
