using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.Data
{
	public sealed class UpdatePoster
	{
		private readonly SemaphoreSlim _semaphore;

		private readonly ILogger<UpdatePoster> _logger;
		private readonly DiscordChannel _channel;

		public UpdatePoster(ILogger<UpdatePoster> logger,DiscordChannel channel)
		{
			this._logger = logger;
			this._channel = channel;
			this._semaphore = new(1, 1);
		}

		public async Task PostUpdatesAsync(IReadOnlyList<DiscordEmbedBuilder> embeds)
		{
			await this._semaphore.WaitAsync();
			try
			{
				for (var i = 0; i < embeds.Count; i++)
				{
					var embed = embeds[i];
					this._logger.LogTrace($"Posting update to {this._channel} - {embed}");
					await this._channel.SendMessageAsync(embed: embed);
				}
			}
			finally
			{
				this._semaphore.Release();
			}
		}
	}
}