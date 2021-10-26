#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using DSharpPlus;
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

namespace PaperMalKing
{
	public static class Program
	{
		public static Task Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();

			return host.RunAsync();
		}

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
			{
				services.AddDbContext<DatabaseContext>();
				var config = hostContext.Configuration;

				services.AddOptions<DiscordOptions>().Bind(config.GetSection(DiscordOptions.Discord));
				services.AddOptions<CommandsOptions>().Bind(config.GetSection(CommandsOptions.Commands));
				services.AddOptions<DatabaseOptions>().Bind(config.GetSection(DatabaseOptions.Database));
				services.AddSingleton<DiscordClient>(provider =>
				{
					var options = provider.GetRequiredService<IOptions<DiscordOptions>>();
					var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
					var cfg = new DiscordConfiguration
					{
						Intents = DiscordIntents.Guilds | DiscordIntents.GuildMembers | DiscordIntents.GuildMessages,
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
}