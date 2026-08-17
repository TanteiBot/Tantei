// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Api;

public static class ApiEndpoints
{
	public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapAuthEndpoints();
		endpoints.MapGuildEndpoints();
		return endpoints;
	}
}
