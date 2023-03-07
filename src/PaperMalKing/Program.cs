using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Startup;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

Delegate handler = ([Authorize(AuthenticationSchemes = "Discord")](HttpContext context) => Task.FromResult(context.TraceIdentifier));
app.MapGet("discord", handler);
app.MapGet("api/getUpdateTimes", (IEnumerable<IUpdateProvider> updateProviders) => updateProviders.Select(up => new
{
	up.Name,
	InProgress = up.IsUpdateInProgress,
	NextIn = up.DateTimeOfNextUpdate > DateTimeOffset.UtcNow ? up.DateTimeOfNextUpdate - DateTimeOffset.UtcNow : default }));
app.MapFallbackToFile("index.html");

app.Run();