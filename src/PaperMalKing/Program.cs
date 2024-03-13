// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Startup;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSpaStaticFiles(options => options.RootPath = "wwwroot");
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(options => options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
	options.LoginPath = "/signin";
	options.LogoutPath = "/signout";
}).AddDiscord("Discord", options =>
{
	options.CallbackPath = new("/auth/oauthDiscord");
	options.ClientId = builder.Configuration.GetValue<string>("Discord:ClientId")!;
	options.ClientSecret = builder.Configuration.GetValue<string>("Discord:ClientSecret")!;
	options.SaveTokens = true;
	options.Scope.Add("identify");
	options.Scope.Add("guilds");
	options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
	options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
});
builder.Host.ConfigureBotServices();
builder.Host.ConfigureBotHost();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
const string spaPath = "/app";
app.Map(new PathString(spaPath), client =>
{
	client.UseSpaStaticFiles();
	client.UseSpa(spa =>
	{
		spa.Options.SourcePath = "clientapp";

		// adds no-store header to index page to prevent deployment issues (prevent linking to old .js files)
		// .js and other static resources are still cached by the browser
		spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
		{
			OnPrepareResponse = ctx =>
			{
				var headers = ctx.Context.Response.GetTypedHeaders();
				headers.CacheControl = new()
				{
					NoCache = true,
					NoStore = true,
					MustRevalidate = true,
				};
			},
		};
	});
});

Delegate handler =
	[Authorize(AuthenticationSchemes = "Discord")]
	(HttpContext context) => Task.FromResult(context.TraceIdentifier);
app.MapGet("discord", handler);
app.MapGet("api/getUpdateTimes", (IEnumerable<IUpdateProvider> updateProviders) => updateProviders.Select(up => new
{
	up.Name,
	InProgress = up.IsUpdateInProgress,
	NextIn = up.DateTimeOfNextUpdate > TimeProvider.System.GetUtcNow() ? up.DateTimeOfNextUpdate - TimeProvider.System.GetUtcNow() : default,
}));
app.Run();