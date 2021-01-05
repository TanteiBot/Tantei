using System.Threading.Tasks;
using DSharpPlus;
using Humanizer.Inflections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Options;
using PaperMalKing.Services;
using PaperMalKing.Services.Background;
using Serilog;

namespace PaperMalKing
{
	public static class Program
	{
		public static Task Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();
			using (var scope = host.Services.CreateScope())
			{
				scope.ServiceProvider.GetRequiredService<CommandsService>();
				var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
				db.Database.Migrate();
				var s = scope.ServiceProvider.GetRequiredService<UpdatePublishingService>();
			}

			return host.RunAsync();
		}

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
			{
				Vocabularies.Default.AddPlural("ch", "chs.");
				Vocabularies.Default.AddPlural("v", "vs.");
				Vocabularies.Default.AddPlural("ep", "eps.");

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
						MessageCacheSize = 256
					};
					return new(cfg);
				});
				services.AddSingleton<UpdatePublishingService>();
				services.AddSingleton<CommandsService>();
				services.AddSingleton<UpdateProvidersConfigurationService>();
				services.AddSingleton<GuildManagementService>();
				UpdateProvidersConfigurationService.ConfigureProviders(config, services);

				services.AddHostedService<UpdateProvidersManagementService>();
				services.AddHostedService<DiscordBackgroundService>();
			}).UseSerilog((context, _, configuration) =>
			{
				configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo
							 .Console(outputTemplate: "[{Timestamp:dd.MM.yy HH\\:mm\\:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}");
			});
		}
	}
}