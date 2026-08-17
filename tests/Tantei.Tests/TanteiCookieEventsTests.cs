// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.Startup.Web;

namespace Tantei.Tests;

public sealed class TanteiCookieEventsTests
{
	private static TanteiCookieEvents CreateEvents() => new(null!, null!, NullLogger<TanteiCookieEvents>.Instance);

	private static RedirectContext<CookieAuthenticationOptions> CreateRedirectContext(string path, string redirectUri)
	{
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Path = path;
		var scheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, displayName: null, typeof(CookieAuthenticationHandler));
		return new(httpContext, scheme, new(), new AuthenticationProperties(), redirectUri);
	}

	[Test]
	public async Task RedirectToLoginReturnsUnauthorizedForApiRequests()
	{
		var context = CreateRedirectContext("/api/guilds/manageable", "https://example.com/signin-discord");

		await CreateEvents().RedirectToLogin(context);

		await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status401Unauthorized);
		await Assert.That(context.Response.Headers.ContainsKey("Location")).IsFalse();
	}

	[Test]
	public async Task RedirectToLoginRedirectsForNonApiRequests()
	{
		var context = CreateRedirectContext("/guilds", "https://example.com/signin-discord");

		await CreateEvents().RedirectToLogin(context);

		await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status302Found);
		await Assert.That(context.Response.Headers.Location.ToString()).IsEqualTo("https://example.com/signin-discord");
	}

	[Test]
	public async Task RedirectToAccessDeniedReturnsForbiddenForApiRequests()
	{
		var context = CreateRedirectContext("/api/guilds/manageable", "https://example.com/access-denied");

		await CreateEvents().RedirectToAccessDenied(context);

		await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status403Forbidden);
		await Assert.That(context.Response.Headers.ContainsKey("Location")).IsFalse();
	}

	[Test]
	public async Task RedirectToAccessDeniedRedirectsForNonApiRequests()
	{
		var context = CreateRedirectContext("/guilds", "https://example.com/access-denied");

		await CreateEvents().RedirectToAccessDenied(context);

		await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status302Found);
		await Assert.That(context.Response.Headers.Location.ToString()).IsEqualTo("https://example.com/access-denied");
	}
}
