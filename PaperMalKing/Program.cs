// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Options;
using PaperMalKing.Services;
using PaperMalKing.Services.Background;
using PaperMalKing.UpdatesProviders.Base;
using Serilog;

namespace PaperMalKing;

public static class Program
{
	public static Task Main(string[] args)
	{
		var host = CreateHostBuilder(args).Build();
		host.Services.GetRequiredService<DatabaseContext>().Database.Migrate();
		return host.RunAsync();
	}

	private static IHostBuilder CreateHostBuilder(string[] args)
	{
		return Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
		{
			services.AddDbContext<DatabaseContext>();
			var config = hostContext.Configuration;

			services.AddOptions<DiscordOptions>().Bind(config.GetSection(DiscordOptions.Discord));
			services.AddOptions<DatabaseOptions>().Bind(config.GetSection(DatabaseOptions.Database));
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
		}).UseSerilog((context, _, configuration) =>
		{
			configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo.Console(
				outputTemplate:
				"[{Timestamp:dd.MM.yy HH\\:mm\\:ss.fff} {Level:u3}] [{SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}");
		});
	}
}