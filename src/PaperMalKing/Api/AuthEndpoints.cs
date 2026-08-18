// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using PaperMalKing.Api.Contracts;
using PaperMalKing.Startup.Web;
using PaperMalKing.Startup.Web.Guilds;
using PaperMalKing.Startup.Web.Tokens;

namespace PaperMalKing.Api;

internal static class AuthEndpoints
{
	public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/auth");

		group.MapGet("/login", ChallengeHttpResult (string? returnUrl) => TypedResults.Challenge(
				 new AuthenticationProperties { RedirectUri = LoginRedirects.SanitizeReturnUrl(returnUrl), },
				 [DiscordAuthenticationDefaults.AuthenticationScheme]))
			 .AllowAnonymous();

		group.MapPost("/logout", async Task<NoContent> (HttpContext context, DiscordOAuthTokenStore tokenStore, UserGuildsCache guildsCache) =>
			 {
				 if (ulong.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var discordUserId))
				 {
					 await tokenStore.DeleteAsync(discordUserId, context.RequestAborted);
					 guildsCache.Evict(discordUserId);
				 }

				 await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				 return TypedResults.NoContent();
			 })
			 .AllowAnonymous();

		group.MapGet("/me", GetCurrentUser)
			 .AllowAnonymous();

		return endpoints;
	}

	private static Results<Ok<CurrentUserResponse>, UnauthorizedHttpResult> GetCurrentUser(HttpContext context)
	{
		var user = context.User;
		var discordUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
		if (user.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(discordUserId))
		{
			return TypedResults.Unauthorized();
		}

		var avatarHash = user.FindFirstValue("urn:discord:avatar");
		var avatarUrl = avatarHash is null ? null : $"https://cdn.discordapp.com/avatars/{discordUserId}/{avatarHash}.png";

		return TypedResults.Ok(new CurrentUserResponse(discordUserId,
													   user.FindFirstValue(ClaimTypes.Name) ?? "",
													   avatarUrl,
													   string.Equals(user.FindFirstValue(TanteiClaimTypes.Registered), "true", StringComparison.Ordinal),
													   string.Equals(user.FindFirstValue(TanteiClaimTypes.WebAdmin), "true", StringComparison.Ordinal)));
	}
}
