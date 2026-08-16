// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Reflection;
using PaperMalKing.Api;
using PaperMalKing.Startup;
using PaperMalKing.Startup.Web;

var builder = WebApplication.CreateBuilder(args);

var isGeneratingOpenApiDocument = string.Equals(
	Assembly.GetEntryAssembly()?.GetName().Name,
	"GetDocument.Insider",
	StringComparison.Ordinal);

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

builder.Services.AddWebAuthentication(builder.Configuration, builder.Environment);

builder.Services.AddOpenApi();

builder.Host.UseDefaultServiceProvider(c =>
{
	c.ValidateOnBuild = !isGeneratingOpenApiDocument;
	c.ValidateScopes = true;
});

if (!isGeneratingOpenApiDocument)
{
	builder.Host.ConfigureBotServices();
	builder.Host.ConfigureBotHost();
}

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
	app.UseHsts();

	app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.MapApiEndpoints();
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();
