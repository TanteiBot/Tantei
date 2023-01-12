// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
#pragma warning disable CA1852
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Database.CompiledModels;
using PaperMalKing.Options;
using PaperMalKing.Services;
using PaperMalKing.Services.Background;
using PaperMalKing.UpdatesProviders.Base;
using Serilog;

await Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
{
	services.AddDbContextFactory<DatabaseContext>((services, builder) =>
	{
		builder.UseSqlite(services.GetRequiredService<IConfiguration>().GetConnectionString("Default"),
			o => o.MigrationsAssembly("PaperMalKing.Database.Migrations")).UseModel(DatabaseContextModel.Instance);
	});
	var config = hostContext.Configuration;

	services.AddOptions<DiscordOptions>().Bind(config.GetSection(DiscordOptions.Discord));
	services.AddSingleton<DiscordClient>(provider =>
	{
		var options = provider.GetRequiredService<IOptions<DiscordOptions>>();
		var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
		var cfg = new DiscordConfiguration
		{
			Intents = DiscordIntents.Guilds | DiscordIntents.GuildMembers,
			Token = options.Value.Token,
			AutoReconnect = true,
			LoggerFactory = loggerFactory,
			ReconnectIndefinitely = true,
			MessageCacheSize = 256,
			MinimumLogLevel = LogLevel.Trace
		};
		return new(cfg);
	});
	services.AddSingleton<UpdatePublishingService>();
	services.AddSingleton<ICommandsService, CommandsService>();
	services.AddSingleton<UpdateProvidersConfigurationService>();
	services.AddSingleton<GuildManagementService>();
	UpdateProvidersConfigurationService.ConfigureProviders(config, services);

	services.AddHostedService<UpdateProvidersManagementService>();
	services.AddHostedService<DiscordBackgroundService>();
	services.AddHostedService<OnStartupActionsExecutingService>();
	services.AddSingleton<UserCleanupService>();
	services.AddSingleton<GeneralUserService>();
	RunSQLiteConfiguration();
}).UseSerilog((context, _, configuration) =>
{
	configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo.Console(
		outputTemplate: "[{Timestamp:dd.MM.yy HH\\:mm\\:ss.fff} {Level:u3}] [{SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}");
}).Build().RunAsync().ConfigureAwait(false);

static void RunSQLiteConfiguration()
{
	SQLitePCL.Batteries_V2.Init();
	// SQLITE_CONFIG_MULTITHREAD
	// https://github.com/dotnet/efcore/issues/9994
	// https://sqlite.org/threadsafe.html
	SQLitePCL.raw.sqlite3_config(2);
}