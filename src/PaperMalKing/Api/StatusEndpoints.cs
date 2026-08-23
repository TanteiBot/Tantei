// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.AspNetCore.Http.HttpResults;
using PaperMalKing.Api.Contracts.Responses;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Api;

internal static class StatusEndpoints
{
	public static RouteGroupBuilder MapStatusEndpoints(this RouteGroupBuilder api)
	{
		api.MapGet("/getUpdateTimes", Ok<UpdateProviderStatusResponse[]> (IEnumerable<BaseUpdateProvider> updateProviders, TimeProvider timeProvider) => TypedResults.Ok(updateProviders.Select(up =>
		{
			var now = timeProvider.GetUtcNow();
			return new UpdateProviderStatusResponse(up.Name, up.IsUpdateInProgress, up.DateTimeOfNextUpdate > now ? up.DateTimeOfNextUpdate - now : null);
		}).ToArray())).WithName("GetUpdateTimes").WithTags("Status");

		api.MapGet("/ping", Ok<PingResponse> (TimeProvider timeProvider) => TypedResults.Ok(new PingResponse("pong", timeProvider.GetUtcNow())))
		   .WithName("Ping")
		   .WithTags("Status");

		((IEndpointConventionBuilder)api).Finally(AuthorizationProblemResponses.Add);

		return api;
	}
}