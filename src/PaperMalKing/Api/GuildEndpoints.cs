// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using PaperMalKing.Api.Contracts;
using PaperMalKing.Startup.Web;
using PaperMalKing.Startup.Web.Guilds;
using PaperMalKing.Startup.Web.Tokens;

namespace PaperMalKing.Api;

internal static class GuildEndpoints
{
	public static IEndpointRouteBuilder MapGuildEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/guilds").WithTags("Guilds");

		group.MapGet("/manageable", Ok<IReadOnlyList<ManageableGuildResponse>> (HttpContext context, GuildQueryService guildQueryService) =>
		{
			var discordUserId = ParseUserId(context);
			var guilds = guildQueryService.GetManageableGuilds(discordUserId);
			return TypedResults.Ok<IReadOnlyList<ManageableGuildResponse>>(
				[.. guilds.Select(g => new ManageableGuildResponse(g.GuildId.ToString(CultureInfo.InvariantCulture), g.Name, g.IconUrl))]);
		}).RequireAuthorization()
		  .WithName("GetManageableGuilds");

		group.MapGet("/invitable", Ok<IReadOnlyList<InvitableGuildResponse>> (HttpContext context, GuildQueryService guildQueryService) =>
		{
			var discordUserId = ParseUserId(context);
			var guilds = guildQueryService.GetInvitableGuilds(discordUserId);
			return TypedResults.Ok<IReadOnlyList<InvitableGuildResponse>>(
				[.. guilds.Select(g => new InvitableGuildResponse(g.GuildId.ToString(CultureInfo.InvariantCulture), g.Name, g.IconUrl))]);
		}).RequireAuthorization(TanteiPolicies.SignedIn)
		  .WithName("GetInvitableGuilds");

		group.MapPost("/refresh", async Task<Results<NoContent, ProblemHttpResult>> (HttpContext context,
																						 DiscordTokenRefreshService tokenRefreshService,
																						 DiscordUserGuildsClient guildsClient,
																						 UserGuildsCache cache) =>
		{
			var discordUserId = ParseUserId(context);
			var accessToken = await tokenRefreshService.GetValidAccessTokenAsync(discordUserId, context.RequestAborted);
			if (accessToken is null)
			{
				return TypedResults.Problem(detail: "The stored Discord authorization is no longer valid, sign in again.",
											statusCode: StatusCodes.Status401Unauthorized,
											title: "Unauthorized");
			}

			IReadOnlyList<DiscordPartialGuild> guilds;
			try
			{
				guilds = await guildsClient.GetGuildsAsync(accessToken, context.RequestAborted);
			}
			catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException or JsonException) &&
										!context.RequestAborted.IsCancellationRequested)
			{
				return TypedResults.Problem(detail: "Discord could not be reached while refreshing the guild list.",
											statusCode: (int)HttpStatusCode.BadGateway,
											title: "Bad Gateway");
			}

			cache.Set(discordUserId, guilds);
			return TypedResults.NoContent();
		}).RequireAuthorization(TanteiPolicies.SignedIn)
		  .ProducesProblem(StatusCodes.Status502BadGateway)
		  .WithName("RefreshGuilds");

		return endpoints;
	}

	private static ulong ParseUserId(HttpContext context)
		=> ulong.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!, CultureInfo.InvariantCulture);
}
