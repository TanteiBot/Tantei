// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Api;

internal static class ApiEndpoints
{
	public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var api = endpoints.MapGroup("/api");

		api.MapAuthEndpoints();
		api.MapConfigEndpoints();
		api.MapCreditsEndpoints();
		api.MapGuildEndpoints();
		api.MapStatusEndpoints();

		((IEndpointConventionBuilder)api).Finally(AuthorizationProblemResponses.Add);

		return endpoints;
	}
}
