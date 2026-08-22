// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.AspNetCore.Http.HttpResults;
using PaperMalKing.Api.Contracts.Responses;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Api;

internal static class ApiEndpoints
{
	public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var api = endpoints.MapGroup("/api");

		api.MapAuthEndpoints();
		api.MapGuildEndpoints();

		api.MapGet("/getUpdateTimes", Ok<UpdateProviderStatusResponse[]> (IEnumerable<BaseUpdateProvider> updateProviders) => TypedResults.Ok(updateProviders.Select(up =>
		{
			var now = TimeProvider.System.GetUtcNow();
			return new UpdateProviderStatusResponse(up.Name, up.IsUpdateInProgress, up.DateTimeOfNextUpdate > now ? up.DateTimeOfNextUpdate - now : null);
		}).ToArray())).WithName("GetUpdateTimes").WithTags("Status");

		api.MapGet("/ping", Ok<PingResponse> () => TypedResults.Ok(new PingResponse("pong", TimeProvider.System.GetUtcNow())))
		   .WithName("Ping")
		   .WithTags("Status");

		((IEndpointConventionBuilder)api).Finally(AuthorizationProblemResponses.Add);

		return endpoints;
	}
}
