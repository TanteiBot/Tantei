// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.AspNetCore.Http.HttpResults;
using PaperMalKing.Api.Contracts.Responses;

namespace PaperMalKing.Api;

internal static class CreditsEndpoints
{
	private const string CacheControlValue = "public, max-age=3600";

	public static RouteGroupBuilder MapCreditsEndpoints(this RouteGroupBuilder api)
	{
		api.MapGet("/credits", Ok<CreditsResponse> (CreditsProvider creditsProvider, HttpResponse response) =>
		   {
			   response.Headers.CacheControl = CacheControlValue;

			   return TypedResults.Ok(creditsProvider.Credits);
		   })
		   .AllowAnonymous()
		   .WithName("GetCredits")
		   .WithTags("Credits");

		return api;
	}
}
