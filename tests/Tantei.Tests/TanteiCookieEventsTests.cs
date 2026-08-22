// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using EntityFramework.Exceptions.Sqlite;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.Database;
using PaperMalKing.Startup.Web;

namespace Tantei.Tests;

public sealed class TanteiCookieEventsTests
{
	private const ulong RegisteredUserId = 111UL;

	private const ulong UnregisteredUserId = 222UL;

	private static TanteiCookieEvents CreateEvents(IDbContextFactory<DatabaseContext> dbContextFactory,
												   IApplicationOwners applicationOwners,
												   IProblemDetailsService? problemDetailsService = null) =>
		new(dbContextFactory, applicationOwners, problemDetailsService ?? new FakeProblemDetailsService(), NullLogger<TanteiCookieEvents>.Instance);

	private static RedirectContext<CookieAuthenticationOptions> CreateRedirectContext(string path, string redirectUri)
	{
		var httpContext = new DefaultHttpContext();
		httpContext.Request.Path = path;
		var scheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, displayName: null, typeof(CookieAuthenticationHandler));
		return new(httpContext, scheme, new(), new AuthenticationProperties(), redirectUri);
	}

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	private static async Task<(IDbContextFactory<DatabaseContext> Factory, SqliteConnection Connection)> CreateDbContextFactoryAsync()
	{
		var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync();

		var services = new ServiceCollection();
		services.AddDbContextFactory<DatabaseContext>(o => o.UseSqlite(connection).UseExceptionProcessor());
		var provider = services.BuildServiceProvider();

		var factory = provider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
		using (var db = factory.CreateDbContext())
		{
			await db.Database.EnsureCreatedAsync();
			db.DiscordUsers.Add(new()
			{
				DiscordUserId = RegisteredUserId,
				BotUser = new(),
				Guilds = [],
			});
			db.SaveChanges();
		}

		return (factory, connection);
	}

	private static CookieValidatePrincipalContext CreateValidatePrincipalContext(HttpContext httpContext, ClaimsPrincipal principal)
	{
		var scheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, displayName: null, typeof(CookieAuthenticationHandler));
		var ticket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
		return new(httpContext, scheme, new(), ticket);
	}

	private static DefaultHttpContext CreateHttpContext(FakeAuthenticationService authenticationService)
	{
		var services = new ServiceCollection();
		services.AddSingleton<IAuthenticationService>(authenticationService);
		var provider = services.BuildServiceProvider();
		return new() { RequestServices = provider };
	}

	private static ClaimsPrincipal CreatePrincipal(string? nameIdentifier, bool forgeRegistered = false, bool forgeWebAdmin = false)
	{
		var claims = new List<Claim>();
		if (nameIdentifier is not null)
		{
			claims.Add(new(ClaimTypes.NameIdentifier, nameIdentifier));
		}

		if (forgeRegistered)
		{
			claims.Add(new(TanteiClaimTypes.Registered, "true"));
		}

		if (forgeWebAdmin)
		{
			claims.Add(new(TanteiClaimTypes.WebAdmin, "true"));
		}

		var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
		return new(identity);
	}

	[Test]
	public async Task RedirectToLoginReturnsUnauthorizedProblemForApiRequests()
	{
		var context = CreateRedirectContext("/api/guilds/manageable", "https://example.com/signin-discord");
		var problemDetailsService = new FakeProblemDetailsService();

		await CreateEvents(null!, null!, problemDetailsService).RedirectToLogin(context);

		await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status401Unauthorized);
		await Assert.That(context.Response.Headers.ContainsKey("Location")).IsFalse();
		await Assert.That(problemDetailsService.Written?.Status).IsEqualTo(StatusCodes.Status401Unauthorized);
		await Assert.That(problemDetailsService.Written?.Title).IsEqualTo("Unauthorized");
		await Assert.That(problemDetailsService.Written?.Detail).IsNotNull();
	}

	[Test]
	public async Task RedirectToLoginRedirectsForNonApiRequests()
	{
		var context = CreateRedirectContext("/guilds", "https://example.com/signin-discord");

		await CreateEvents(null!, null!).RedirectToLogin(context);

		await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status302Found);
		await Assert.That(context.Response.Headers.Location.ToString()).IsEqualTo("https://example.com/signin-discord");
	}

	[Test]
	public async Task RedirectToAccessDeniedReturnsForbiddenProblemForApiRequests()
	{
		var context = CreateRedirectContext("/api/guilds/manageable", "https://example.com/access-denied");
		var problemDetailsService = new FakeProblemDetailsService();

		await CreateEvents(null!, null!, problemDetailsService).RedirectToAccessDenied(context);

		await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status403Forbidden);
		await Assert.That(context.Response.Headers.ContainsKey("Location")).IsFalse();
		await Assert.That(problemDetailsService.Written?.Status).IsEqualTo(StatusCodes.Status403Forbidden);
		await Assert.That(problemDetailsService.Written?.Title).IsEqualTo("Forbidden");
		await Assert.That(problemDetailsService.Written?.Detail).IsNotNull();
	}

	[Test]
	public async Task RedirectToAccessDeniedRedirectsForNonApiRequests()
	{
		var context = CreateRedirectContext("/guilds", "https://example.com/access-denied");

		await CreateEvents(null!, null!).RedirectToAccessDenied(context);

		await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status302Found);
		await Assert.That(context.Response.Headers.Location.ToString()).IsEqualTo("https://example.com/access-denied");
	}

	[Test]
	public async Task ForgedRegisteredClaimIsStrippedForUnregisteredUser()
	{
		var (factory, connection) = await CreateDbContextFactoryAsync();
		await using var ownedConnection = connection;

		var events = CreateEvents(factory, new FakeApplicationOwners(isOwner: false));
		var httpContext = CreateHttpContext(new FakeAuthenticationService());
		var principal = CreatePrincipal(UnregisteredUserId.ToString(System.Globalization.CultureInfo.InvariantCulture), forgeRegistered: true);
		var context = CreateValidatePrincipalContext(httpContext, principal);

		await events.ValidatePrincipal(context);

		await Assert.That(context.Principal!.FindFirstValue(TanteiClaimTypes.Registered)).IsEqualTo("false");
	}

	[Test]
	public async Task GenuinelyRegisteredUserEndsUpRegisteredTrue()
	{
		var (factory, connection) = await CreateDbContextFactoryAsync();
		await using var ownedConnection = connection;

		var events = CreateEvents(factory, new FakeApplicationOwners(isOwner: false));
		var httpContext = CreateHttpContext(new FakeAuthenticationService());
		var principal = CreatePrincipal(RegisteredUserId.ToString(System.Globalization.CultureInfo.InvariantCulture));
		var context = CreateValidatePrincipalContext(httpContext, principal);

		await events.ValidatePrincipal(context);

		await Assert.That(context.Principal!.FindFirstValue(TanteiClaimTypes.Registered)).IsEqualTo("true");
	}

	[Test]
	public async Task WebAdminClaimTracksIsOwner()
	{
		var (factory, connection) = await CreateDbContextFactoryAsync();
		await using var ownedConnection = connection;

		var events = CreateEvents(factory, new FakeApplicationOwners(isOwner: true));
		var httpContext = CreateHttpContext(new FakeAuthenticationService());
		var principal = CreatePrincipal(UnregisteredUserId.ToString(System.Globalization.CultureInfo.InvariantCulture), forgeWebAdmin: false);
		var context = CreateValidatePrincipalContext(httpContext, principal);

		await events.ValidatePrincipal(context);

		await Assert.That(context.Principal!.FindFirstValue(TanteiClaimTypes.WebAdmin)).IsEqualTo("true");
	}

	[Test]
	public async Task ForgedWebAdminClaimIsStrippedWhenNotAnOwner()
	{
		var (factory, connection) = await CreateDbContextFactoryAsync();
		await using var ownedConnection = connection;

		var events = CreateEvents(factory, new FakeApplicationOwners(isOwner: false));
		var httpContext = CreateHttpContext(new FakeAuthenticationService());
		var principal = CreatePrincipal(UnregisteredUserId.ToString(System.Globalization.CultureInfo.InvariantCulture), forgeWebAdmin: true);
		var context = CreateValidatePrincipalContext(httpContext, principal);

		await events.ValidatePrincipal(context);

		await Assert.That(context.Principal!.FindFirstValue(TanteiClaimTypes.WebAdmin)).IsEqualTo("false");
	}

	[Test]
	public async Task UnparsableNameIdentifierIsRejectedAndSignedOut()
	{
		var (factory, connection) = await CreateDbContextFactoryAsync();
		await using var ownedConnection = connection;

		var events = CreateEvents(factory, new FakeApplicationOwners(isOwner: false));
		var authenticationService = new FakeAuthenticationService();
		var httpContext = CreateHttpContext(authenticationService);
		var principal = CreatePrincipal("not-a-ulong");
		var context = CreateValidatePrincipalContext(httpContext, principal);

		await events.ValidatePrincipal(context);

		await Assert.That(context.Principal).IsNull();
		await Assert.That(authenticationService.SignOutCallCount).IsEqualTo(1);
	}

	private sealed class FakeProblemDetailsService : IProblemDetailsService
	{
		public ProblemDetails? Written { get; private set; }

		public ValueTask WriteAsync(ProblemDetailsContext context)
		{
			this.Written = context.ProblemDetails;
			return ValueTask.CompletedTask;
		}
	}

	private sealed class FakeApplicationOwners(bool isOwner) : IApplicationOwners
	{
		public bool IsOwner(ulong discordUserId) => isOwner;
	}

	private sealed class FakeAuthenticationService : IAuthenticationService
	{
		public int SignOutCallCount { get; private set; }

		public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) => throw new NotSupportedException();

		public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => throw new NotSupportedException();

		public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => throw new NotSupportedException();

		public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) =>
			throw new NotSupportedException();

		public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
		{
			this.SignOutCallCount++;
			return Task.CompletedTask;
		}
	}
}
