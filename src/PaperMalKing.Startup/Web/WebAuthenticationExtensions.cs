// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PaperMalKing.Startup.Options;
using PaperMalKing.Startup.Web.Guilds;
using PaperMalKing.Startup.Web.Tokens;

namespace PaperMalKing.Startup.Web;

public static class WebAuthenticationExtensions
{
	public static IServiceCollection AddWebAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		var webOptions = configuration.GetSection(WebOptions.Web).Get<WebOptions>() ?? new WebOptions();

		services.AddOptions<WebOptions>().BindConfiguration(WebOptions.Web).ValidateDataAnnotations().ValidateOnStart();
		services.TryAddSingleton(TimeProvider.System);
		services.AddMemoryCache();
		services.AddSingleton<DiscordOAuthTokenStore>();
		services.AddSingleton<TanteiCookieEvents>();
		services.AddSingleton<IApplicationOwners, DiscordApplicationOwners>();
		services.AddSingleton<IBotGuildPresence, BotGuildPresence>();
		services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, GuildAdminAuthorizationHandler>();

		services.Configure<ForwardedHeadersOptions>(options =>
		{
			options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
			options.KnownIPNetworks.Clear();
			options.KnownProxies.Clear();
		});

		services.AddDataProtection().SetApplicationName("Tantei")
#if IsInContainer
				.PersistKeysToFileSystem(new(CreateDataProtectionKeysDirectory(webOptions)))
#endif
			;

		var cookieLifetime = TimeSpan.FromDays(webOptions.CookieLifetimeInDays);

		services.AddAuthentication(options =>
		{
			options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
		}).AddCookie(options =>
		{
			options.Cookie.Name = "tantei.session";
			options.Cookie.HttpOnly = true;
			options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
			options.Cookie.SameSite = SameSiteMode.Lax;
			options.ExpireTimeSpan = cookieLifetime;
			options.SlidingExpiration = true;
			options.EventsType = typeof(TanteiCookieEvents);
		}).AddDiscord(options =>
		{
			var discord = configuration.GetSection(DiscordOptions.Discord);
			options.ClientId = discord.GetValue<string>(nameof(DiscordOptions.ClientId))!;
			options.ClientSecret = discord.GetValue<string>(nameof(DiscordOptions.ClientSecret))!;
			options.CallbackPath = new("/signin-discord");
			options.SaveTokens = false;
			options.Scope.Clear();
			options.Scope.Add("identify");
			options.Scope.Add("guilds");
			options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
			options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
			options.ClaimActions.MapJsonKey("urn:discord:avatar", "avatar");
		});

		var registeredPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireClaim(TanteiClaimTypes.Registered, "true").Build();

		services.AddAuthorizationBuilder().SetDefaultPolicy(registeredPolicy).SetFallbackPolicy(registeredPolicy)
				.AddPolicy(TanteiPolicies.SignedIn, p => p.RequireAuthenticatedUser())
				.AddPolicy(TanteiPolicies.Registered, p => p.RequireAuthenticatedUser().RequireClaim(TanteiClaimTypes.Registered, "true"))
				.AddPolicy(TanteiPolicies.WebAdmin, p => p.RequireAuthenticatedUser().RequireClaim(TanteiClaimTypes.WebAdmin, "true"))
				.AddPolicy(TanteiPolicies.GuildAdmin, p => p.RequireAuthenticatedUser().AddRequirements(new GuildAdminRequirement()));

		return services;
	}

#if IsInContainer
	private static string CreateDataProtectionKeysDirectory(WebOptions webOptions)
	{
		var keysDirectory = webOptions.DataProtectionKeysDirectory ??
							Path.Combine(Environment.GetEnvironmentVariable("TANTEI_CONFIG_DIR") ?? "/config", "dataprotection-keys");
		Directory.CreateDirectory(keysDirectory);
		return keysDirectory;
	}
#endif
}
