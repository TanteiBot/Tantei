// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using PaperMalKing.Api.Contracts.Responses;
using PaperMalKing.Startup.Options;
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

		group.MapGet("/invitable",
			async Task<Ok<InvitableGuildsResponse>> (HttpContext context,
													GuildQueryService guildQueryService,
													InviteAuthorization inviteAuthorization,
													UserGuildsProvider userGuildsProvider) =>
			{
				var eligibility = await inviteAuthorization.GetEligibilityAsync(context.User, context.RequestAborted);
				if (eligibility != InviteEligibility.Allowed)
				{
					return TypedResults.Ok(new InvitableGuildsResponse(eligibility, []));
				}

				var discordUserId = ParseUserId(context);
				var userGuilds = await userGuildsProvider.GetGuildsAsync(discordUserId, context.RequestAborted);
				if (userGuilds is null)
				{
					return TypedResults.Ok(new InvitableGuildsResponse(InviteEligibility.Unknown, []));
				}

				var guilds = guildQueryService.GetInvitableGuilds(userGuilds);
				return TypedResults.Ok(new InvitableGuildsResponse(InviteEligibility.Allowed,
					[.. guilds.Select(g => new InvitableGuildResponse(g.GuildId.ToString(CultureInfo.InvariantCulture), g.Name, g.IconUrl))]));
			}).RequireAuthorization(TanteiPolicies.SignedIn)
			  .WithName("GetInvitableGuilds");

		group.MapGet("/{guildId}/invite",
			async Task<Results<RedirectHttpResult, ForbidHttpResult, NotFound>> (HttpContext context,
																				ulong guildId,
																				GuildQueryService guildQueryService,
																				InviteAuthorization inviteAuthorization,
																				UserGuildsProvider userGuildsProvider,
																				IOptions<DiscordOptions> discordOptions) =>
			{
				if (await inviteAuthorization.GetEligibilityAsync(context.User, context.RequestAborted) != InviteEligibility.Allowed)
				{
					return TypedResults.Forbid();
				}

				var discordUserId = ParseUserId(context);
				var userGuilds = await userGuildsProvider.GetGuildsAsync(discordUserId, context.RequestAborted);
				if (userGuilds is null || guildQueryService.GetInvitableGuilds(userGuilds).All(g => g.GuildId != guildId))
				{
					return TypedResults.NotFound();
				}

				var clientId = Uri.EscapeDataString(discordOptions.Value.ClientId);
				return TypedResults.Redirect($"{DiscordApiConstants.AuthorizeUrl}?client_id={clientId}" +
											 $"&scope={DiscordApiConstants.InviteScopes}&permissions={DiscordApiConstants.InvitePermissions}" +
											 $"&guild_id={guildId.ToString(CultureInfo.InvariantCulture)}&disable_guild_select=true");
			}).RequireAuthorization(TanteiPolicies.SignedIn)
			  .Produces(StatusCodes.Status302Found)
			  .Produces(StatusCodes.Status403Forbidden)
			  .Produces(StatusCodes.Status404NotFound)
			  .WithName("InviteToGuild");

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
