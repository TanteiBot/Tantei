// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.UpdateProvider.Installer;
using PaperMalKing.MyAnimeList.UpdateProvider.Installer;
using PaperMalKing.Shikimori.UpdateProvider.Installer;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Startup.Services;

internal sealed class UpdateProvidersConfigurationService
{
	private readonly Dictionary<string, IUpdateProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

	public ReadOnlyDictionary<string, IUpdateProvider> Providers => this._providers.AsReadOnly();

	public UpdateProvidersConfigurationService(ILogger<UpdateProvidersConfigurationService> logger, IEnumerable<IUpdateProvider> updateProviders)
	{
		logger.BuildingUpdateProvidersConfigurationService(typeof(UpdateProvidersConfigurationService));
		foreach (var updateProvider in updateProviders)
		{
			logger.RegisteringUpdateProvider(updateProvider);
			this._providers.Add(updateProvider.Name, updateProvider);
		}

		if (this._providers.Count == 0)
		{
			logger.NoUpdateProvidersRegistered();
		}

		logger.BuiltUpdateProvidersConfigurationService(typeof(UpdateProvidersConfigurationService));
	}

	public static void ConfigureProviders(IConfiguration configuration, IServiceCollection services)
	{
		services.AddAniList(configuration);
		services.AddMyAnimeList(configuration);
		services.AddShikimori(configuration);
	}
}