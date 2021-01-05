using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using PaperMalKing.Utilities;

namespace PaperMalKing.Services
{
	public sealed class UpdateProvidersConfigurationService
	{
		private readonly ILogger<UpdateProvidersConfigurationService> _logger;
		private readonly Dictionary<string, IUpdateProvider> _providers = new();
		private readonly IServiceProvider _serviceProvider;

		public IReadOnlyDictionary<string, IUpdateProvider> Providers => this._providers;

		public UpdateProvidersConfigurationService(ILogger<UpdateProvidersConfigurationService> logger, IServiceProvider serviceProvider)
		{
			this._logger = logger;
			this._logger.LogTrace("Building {@UpdateProvidersConfigurationService}", typeof(UpdateProvidersConfigurationService));
			this._serviceProvider = serviceProvider;
			foreach (var updateProvider in this._serviceProvider.GetServices<IUpdateProvider>())
			{
				this._logger.LogDebug("Registering {@UpdateProvider} update provider", updateProvider);
				this._providers.Add(updateProvider.Name, updateProvider);
			}

			if (!this._providers.Any())
				this._logger.LogCritical("No update providers were registered");

			this._logger.LogTrace("Built {@UpdateProvidersConfigurationService}", typeof(UpdateProvidersConfigurationService));
		}

		public static void ConfigureProviders(IConfiguration configuration, IServiceCollection services)
		{
			foreach (var assembly in Utils.LoadAndListPmkAssemblies())
			{
				foreach (var updateProvider in assembly.GetTypes().Where(t =>
					t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUpdateProviderConfigurator<>))))
					updateProvider.GetMethod("Configure")?.Invoke(null, new object[] {configuration, services});
			}
		}
	}
}