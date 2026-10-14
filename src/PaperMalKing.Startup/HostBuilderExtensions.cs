// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus;
using EntityFramework.Exceptions.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Diagnostics;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Database.CompiledModels;
using PaperMalKing.Startup.Commands;
using PaperMalKing.Startup.Options;
using PaperMalKing.Startup.Services;
using PaperMalKing.Startup.Services.Background;
using PaperMalKing.Startup.Services.ExecuteOnStartup;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Colors;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
#if IsInContainer
using System.Diagnostics.CodeAnalysis;
#else
using Microsoft.Extensions.Hosting.Systemd;
#endif

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
			const int sqliteMultithreadedMode = 2;
			SQLitePCL.raw.sqlite3_config(sqliteMultithreadedMode);
		}

		hostBuilder
		.ConfigureAppConfiguration((_, builder) => builder.AddEnvironmentVariables(prefix: "Tantei_"))
		.ConfigureServices((context, services) =>
		{
			static void ConfigureDbContext(IServiceProvider services, DbContextOptionsBuilder builder)
			{
				var environment = services.GetRequiredService<IHostEnvironment>();

				builder.UseSqlite(services.GetRequiredService<IConfiguration>().GetConnectionString("Default"),
					o => o.MigrationsAssembly("PaperMalKing.Database.Migrations"))
					   .UseModel(DatabaseContextModel.Instance)
					   .UseExceptionProcessor()
					   .ConfigureWarnings(w =>
					   {
						   List<EventId> eventIds =
						   [
							   RelationalEventId.MultipleCollectionIncludeWarning, RelationalEventId.QueryPossibleUnintendedUseOfEqualsWarning,
							   RelationalEventId.AllIndexPropertiesNotMappedToAnyTable,
							   RelationalEventId.IndexPropertiesBothMappedAndNotMappedToTable,
							   RelationalEventId.KeyPropertiesNotMappedToTable, RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables,
							   RelationalEventId.ForeignKeyTpcPrincipalWarning,
						   ];

						   if (environment.IsDevelopment())
						   {
							   w.Throw([.. eventIds])
								.Log((RelationalEventId.PendingModelChangesWarning, LogLevel.Error));
						   }
						   else
						   {
							   eventIds.Add(RelationalEventId.PendingModelChangesWarning);
							   w.Throw([.. eventIds]);
						   }
					   });
			}

			services.AddPooledDbContextFactory<DatabaseContext>(ConfigureDbContext);
			services.AddDbContext<DatabaseContext>(ConfigureDbContext, optionsLifetime: ServiceLifetime.Singleton);
			services.AddSingleton<IExecuteOnStartupService, MigrateOnStartupService>();
			services.AddSingleton<IExecuteOnStartupService, WarnOnSeqOnStartupService>();

			services.AddOptions<DiscordOptions>().BindConfiguration(DiscordOptions.Discord).ValidateDataAnnotations().ValidateOnStart();
			services.AddOptions<SeqOptions>().BindConfiguration(SeqOptions.Seq);
			services.AddOptions<OtlpOptions>().BindConfiguration(OtlpOptions.Otlp);
			services.AddResilienceEnricher();
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
					LogUnknownEvents = false,
				};
				return new(cfg);
			});

			services.AddRedaction();

			services.AddExtendedHttpClientLogging(options =>
			{
				options.LogBody = true;
				options.RequestPathLoggingMode = OutgoingPathLoggingMode.Structured;
				options.RequestPathParameterRedactionMode = HttpRouteParameterRedactionMode.None;
				options.BodyReadTimeout = TimeSpan.FromSeconds(59);

				foreach (var contentType in (ReadOnlySpan<string>)[System.Net.Mime.MediaTypeNames.Application.Json])
				{
					options.RequestBodyContentTypes.Add(contentType);
					options.ResponseBodyContentTypes.Add(contentType);
				}
			});

			services.AddSingleton<UpdatePublishingService>();
			services.AddSingleton<ICommandsService, CommandsService>();
			services.AddSingleton<UpdateProvidersConfigurationService>();
			services.AddSingleton<GuildManagementService>();

			services.AddTransient<AdminCommands>();
			services.AddTransient<GuildManagementCommands>();
			services.AddTransient<UngroupedCommands>();
			UpdateProvidersConfigurationService.ConfigureProviders(services, context.Configuration);

			services.AddHostedService<DiscordBackgroundService>();
			services.AddHostedService<OnStartupActionsExecutingService>();
			services.AddSingleton<UserCleanupService>();
			services.AddSingleton<GeneralUserService>();
			services.AddSingleton(typeof(CustomColorService<,>));
			RunSQLiteConfiguration();
		});
		return hostBuilder;
	}

#if IsInContainer
	[SuppressMessage("Roslynator", "RCS1118:Mark local variable as const", Justification = "Variables are considered consts in container builds")]
	[SuppressMessage("ReSharper", "ConvertToConstant.Local", Justification = "Variables are considered consts in container builds")]
	[SuppressMessage("Critical Code Smell", "S3353:Unchanged variables should be marked as \"const\"", Justification = "Variables are considered consts in container builds")]
#endif
	public static IHostBuilder ConfigureBotHost(this IHostBuilder hostBuilder)
	{
#if !IsInContainer
		if (SystemdHelpers.IsSystemdService())
		{
			hostBuilder = hostBuilder.UseSystemd();
		}
#endif

		hostBuilder.UseSerilog((context, services, configuration) =>
		{
			var level =
#if !IsInContainer
				SystemdHelpers.IsSystemdService() ? "" :
#endif
				"{Level:u3}";

			var template =
				$$"""[{Timestamp:dd.MM.yy HH\\:mm\\:ss.fff} {{level}}] [{SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}""";
			var loggerSinkConfiguration = configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo;

			configuration =
#if !IsInContainer
				SystemdHelpers.IsSystemdService() ?
				loggerSinkConfiguration.Console(formatter: new SystemdTextFormatter(new(template))) :
#endif
				loggerSinkConfiguration.Console(outputTemplate: template, formatProvider: CultureInfo.InvariantCulture);

			var seqOptions = services.GetRequiredService<IOptions<SeqOptions>>();

			if (seqOptions.Value.IsEnabled)
			{
				configuration.WriteTo.OpenTelemetry(ot =>
				{
					ot.Endpoint = seqOptions.Value.IngestionUrl;
					ot.Protocol = OtlpProtocol.HttpProtobuf;
					ot.Headers = new Dictionary<string, string>(1, StringComparer.Ordinal)
					{
						["X-Seq-ApiKey"] = seqOptions.Value.ApiKey,
					};

					const string tanteiName = "Tantei";
					const string tanteiDevName = $"{tanteiName}-dev";
					ot.ResourceAttributes = new Dictionary<string, object>(1, StringComparer.Ordinal)
					{
						["service.name"] = context.HostingEnvironment.IsDevelopment() ? tanteiDevName : tanteiName,
					};
				});
			}

			var otlpOptions = services.GetRequiredService<IOptions<OtlpOptions>>();

			if (otlpOptions.Value.IsEnabled)
			{
				configuration.WriteTo.OpenTelemetry(ot =>
				{
					ot.Endpoint = otlpOptions.Value.IngestionUrl;
					ot.Protocol = OtlpProtocol.HttpProtobuf;
					ot.IncludedData |= IncludedData.SourceContextAttribute;

					if (otlpOptions.Value.AdditionalHeaders is { Count: > 0 })
					{
						ot.Headers = otlpOptions.Value.AdditionalHeaders;
					}

					const string tanteiName = "Tantei";
					const string tanteiDevName = $"{tanteiName}-dev";
					ot.ResourceAttributes = new Dictionary<string, object>(1, StringComparer.Ordinal)
					{
						["service.name"] = context.HostingEnvironment.IsDevelopment() ? tanteiDevName : tanteiName,
					};
				}, ignoreEnvironment: true);
			}
		});

		return hostBuilder;
	}
}