using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaperMalKing.Options;
using PaperMalKing.Services;
using PaperMalKing.Services.Background;

namespace PaperMalKing
{
	public static class Program
	{
		static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, collection) =>
			{
				collection.AddLogging(builder => builder.AddConsole());
				var config = hostContext.Configuration;


				collection.AddOptions<DiscordOptions>().Bind(config.GetSection(DiscordOptions.Discord));
				collection.AddOptions<CommandsOptions>().Bind(config.GetSection(CommandsOptions.Commands));
				collection.AddOptions<TimerOptions>().Bind(config.GetSection(TimerOptions.Timer));

				UpdateProvidersManagementService.ConfigureProviders(config, collection);
				collection.AddSingleton<UpdateProvidersManagementService>();
				collection.AddSingleton<UpdatePublishingService>();
				collection.AddSingleton<CommandsService>();
				
				collection.AddHostedService<DiscordService>();
				collection.AddHostedService<TimerService>();
			});
		}
	}
}