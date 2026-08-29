// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.AspNetCore.Http.HttpResults;
using PaperMalKing.Api.Contracts.Responses;
using PaperMalKing.Startup.Web;

namespace PaperMalKing.Api;

internal static class ConfigEndpoints
{
	public static IEndpointRouteBuilder MapConfigEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapGet("/config",
					 Ok<SiteConfigResponse> (InviteAuthorization inviteAuthorization) => TypedResults.Ok(new SiteConfigResponse(inviteAuthorization.Mode)))
				 .AllowAnonymous()
				 .WithName("GetSiteConfig")
				 .WithTags("Config");

		return endpoints;
	}
}
