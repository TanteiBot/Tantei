#region LICENSE

// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.UpdateProvider;
using PaperMalKing.Shikimori.UpdateProvider;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using PaperMalKing.UpdatesProviders.MyAnimeList;

namespace PaperMalKing.Services
{
	public sealed class UpdateProvidersConfigurationService
	{
		private readonly Dictionary<string, IUpdateProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

		public IReadOnlyDictionary<string, IUpdateProvider> Providers => this._providers;

		public UpdateProvidersConfigurationService(ILogger<UpdateProvidersConfigurationService> logger, IServiceProvider serviceProvider)
		{
			logger.LogTrace("Building {@UpdateProvidersConfigurationService}", typeof(UpdateProvidersConfigurationService));
			foreach (var updateProvider in serviceProvider.GetServices<IUpdateProvider>())
			{
				logger.LogDebug("Registering {@UpdateProvider} update provider", updateProvider);
				this._providers.Add(updateProvider.Name, updateProvider);
			}

			if (!this._providers.Any())
				logger.LogCritical("No update providers were registered");

			logger.LogTrace("Built {@UpdateProvidersConfigurationService}", typeof(UpdateProvidersConfigurationService));
		}

		public static void ConfigureProviders(IConfiguration configuration, IServiceCollection services)
		{
			AniListUpdateProviderConfigurator.Configure(configuration, services);
			MalUpdateProviderConfigurator.Configure(configuration,services);
			ShikiUpdateProviderConfigurator.Configure(configuration,services);
		}
	}
}