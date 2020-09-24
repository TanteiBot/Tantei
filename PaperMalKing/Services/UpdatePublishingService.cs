using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdateProviders.Base;
using PaperMalKing.UpdateProviders.Base.EventArgs;
using PaperMalKing.Services.Background;

namespace PaperMalKing.Services
{
	public sealed class UpdatePublishingService
	{
		private readonly ILogger<UpdatePublishingService> _logger;
		private readonly DiscordService _discordService;
		private readonly Dictionary<ulong, DiscordChannel> _channels;

		public UpdatePublishingService(ILogger<UpdatePublishingService> logger, DiscordService discordService, IServiceProvider serviceProvider)
		{
			this._channels = new Dictionary<ulong, DiscordChannel>();
			this._logger = logger;
			this._discordService = discordService;
			var updateProviders = serviceProvider.GetServices<IUpdateProvider>();
			foreach (var updateProvider in updateProviders)
			{
				updateProvider.UpdateFound += this.PublishUpdates;
			}
		}

		private async Task PublishUpdates(UpdateFoundEventArgs args)
		{
			
		}
	}
}