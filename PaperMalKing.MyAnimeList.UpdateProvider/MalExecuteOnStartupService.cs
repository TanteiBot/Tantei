// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Threading;
using System.Threading.Tasks;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	public sealed class MalExecuteOnStartupService : IExecuteOnStartupService
	{
		private readonly ICommandsService _commandsService;

		public MalExecuteOnStartupService(ICommandsService commandsService)
		{
			this._commandsService = commandsService;
		}

		public Task ExecuteAsync(CancellationToken cancellationToken = default)
		{
			this._commandsService.CommandsExtension.RegisterConverter(new FeatureArgumentConverter<MalUserFeatures>());
			return Task.CompletedTask;
		}
	}
}