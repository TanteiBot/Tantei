// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace PaperMalKing.Api;

internal static class AuthorizationProblemResponses
{
	private static readonly string[] ProblemContentTypes = ["application/problem+json"];

	public static void Add(EndpointBuilder builder)
	{
		if (builder.Metadata.OfType<IAllowAnonymous>().Any())
		{
			return;
		}

		var policy = ResolvePolicy(builder);
		if (policy is null)
		{
			return;
		}

		AddProblemResponse(builder, StatusCodes.Status401Unauthorized);

		if (policy.Requirements.Any(static r => r is not DenyAnonymousAuthorizationRequirement))
		{
			AddProblemResponse(builder, StatusCodes.Status403Forbidden);
		}
	}

	private static AuthorizationPolicy? ResolvePolicy(EndpointBuilder builder)
	{
		var options = builder.ApplicationServices.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
		var authorizeData = builder.Metadata.OfType<IAuthorizeData>().ToArray();
		if (authorizeData.Length == 0)
		{
			return options.FallbackPolicy;
		}

		var policyBuilder = new AuthorizationPolicyBuilder();
		foreach (var policy in authorizeData.Select(data => GetPolicy(options, data)))
		{
			policyBuilder.Combine(policy);
		}

		return policyBuilder.Build();
	}

	private static AuthorizationPolicy GetPolicy(AuthorizationOptions options, IAuthorizeData authorizeData) =>
		string.IsNullOrEmpty(authorizeData.Policy)
			? options.DefaultPolicy
			: options.GetPolicy(authorizeData.Policy) ??
			  throw new InvalidOperationException($"Authorization policy '{authorizeData.Policy}' was not found.");

	private static void AddProblemResponse(EndpointBuilder builder, int statusCode)
	{
		if (builder.Metadata.OfType<IProducesResponseTypeMetadata>().Any(m => m.StatusCode == statusCode))
		{
			return;
		}

		builder.Metadata.Add(new ProducesResponseTypeMetadata(statusCode, typeof(ProblemDetails), ProblemContentTypes));
	}
}
