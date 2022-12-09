// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Threading;
using System.Threading.Tasks;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.AniList.UpdateProvider
{
	public sealed class AniListExecuteOnStartupService : IExecuteOnStartupService
	{
		private readonly ICommandsService _commandsService;

		public AniListExecuteOnStartupService(ICommandsService commandsService)
		{
			this._commandsService = commandsService;
		}

		public Task ExecuteAsync(CancellationToken cancellationToken = default)
		{
			this._commandsService.CommandsExtension.RegisterConverter(new FeatureArgumentConverter<AniListUserFeatures>());
			return Task.CompletedTask;
		}
	}
}