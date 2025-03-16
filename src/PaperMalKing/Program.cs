// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using PaperMalKing.Startup;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

var builder = WebApplication.CreateBuilder(args);

#if IsInContainer
Span<string> appsettingsFiles = ["appsettings.json", "appsettings.Development.json", "appsettings.Staging.json", "appsettings.Production.json"];
foreach (var dir in (Span<string>)[Environment.GetEnvironmentVariable("TANTEI_CONFIG_DIR") ?? "/config"])
{
	foreach (var file in appsettingsFiles)
	{
		builder.Configuration.AddJsonFile(System.IO.Path.Combine(dir, file), optional: true);
	}
}
#endif

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
		spa.Options.DefaultPageStaticFileOptions = new()
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
app.MapGet("api/getUpdateTimes", (IEnumerable<BaseUpdateProvider> updateProviders) => updateProviders.Select(up => new
{
	up.Name,
	InProgress = up.IsUpdateInProgress,
	NextIn = up.DateTimeOfNextUpdate > TimeProvider.System.GetUtcNow() ? up.DateTimeOfNextUpdate - TimeProvider.System.GetUtcNow() : null,
}));
app.Run();