// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Globalization;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Database.CompiledModels;
using PaperMalKing.Startup.Options;
using PaperMalKing.Startup.Services;
using PaperMalKing.Startup.Services.Background;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Colors;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace PaperMalKing.Startup;

public static class HostBuilderExtensions
{
	public static IHostBuilder ConfigureBotServices(this IHostBuilder hostBuilder)
	{
		static void RunSQLiteConfiguration()
		{
			SQLitePCL.Batteries_V2.Init();

			// SQLITE_CONFIG_MULTITHREAD
			// https://github.com/dotnet/efcore/issues/9994
			// https://sqlite.org/threadsafe.html
			SQLitePCL.raw.sqlite3_config(2);
		}

		hostBuilder.ConfigureAppConfiguration(x => x.AddJsonFile("appsetings-shared", optional: true, reloadOnChange: false))
				   .ConfigureServices((hostContext, services) =>
		{
			static void ConfigureDbContext(IServiceProvider services, DbContextOptionsBuilder builder)
			{
				builder.UseSqlite(
					services.GetRequiredService<IConfiguration>().GetConnectionString("Default"),
					o => o.MigrationsAssembly("PaperMalKing.Database.Migrations"))
					   .UseModel(DatabaseContextModel.Instance);
			}

			services.AddDbContextFactory<DatabaseContext>(ConfigureDbContext);
			services.AddDbContext<DatabaseContext>(ConfigureDbContext);
			services.AddSingleton<IExecuteOnStartupService, MigrateOnStartupService>();
			var config = hostContext.Configuration;

			services.AddOptions<DiscordOptions>().BindConfiguration(DiscordOptions.Discord).ValidateDataAnnotations().ValidateOnStart();
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
					MinimumLogLevel = LogLevel.Trace,
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
			services.AddSingleton(typeof(CustomColorService<,>));
			RunSQLiteConfiguration();
		});
		return hostBuilder;
	}

	public static IHostBuilder ConfigureBotHost(this IHostBuilder hostBuilder)
	{
		hostBuilder.UseSystemd().UseSerilog((context, _, configuration) =>
		{
			var template =
				$$"""[{Timestamp:dd.MM.yy HH\\:mm\\:ss.fff} {{(SystemdHelpers.IsSystemdService() ? "" : "{Level:u3}")}}] [{SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}""";
			var loggerSinkConfiguration = configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo;
			configuration = SystemdHelpers.IsSystemdService() ?
				loggerSinkConfiguration.Console(formatter: new SystemdTextFormatter(new(template))) :
				loggerSinkConfiguration.Console(outputTemplate: template, formatProvider: CultureInfo.InvariantCulture);

			var seqSection = context.Configuration.GetSection("Seq");

			var isSeqEnabled = seqSection.GetValue<bool>("IsEnabled", defaultValue: false);
			if (isSeqEnabled)
			{
				configuration.WriteTo.OpenTelemetry(ot =>
				{
					ot.Endpoint = seqSection.GetValue<string>("LogIngestionUrl");
					ot.Protocol = OtlpProtocol.HttpProtobuf;
					ot.Headers = new Dictionary<string, string>(1, StringComparer.Ordinal)
					{
						["X-Seq-ApiKey"] = seqSection.GetValue<string>("ApiKey")!,
					};

					const string tanteiName = "Tantei";
					const string tanteiDevName = $"{tanteiName}-dev";
					ot.ResourceAttributes = new Dictionary<string, object>(1, StringComparer.Ordinal)
					{
						["service.name"] = context.HostingEnvironment.IsDevelopment() ? tanteiDevName : tanteiName,
					};
				});
			}
		});
		return hostBuilder;
	}
}