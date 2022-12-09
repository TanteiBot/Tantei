// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

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

		public UpdatePoster(ILogger<UpdatePoster> logger, DiscordChannel channel)
		{
			this._logger = logger;
			this._channel = channel;
			this._semaphore = new(1, 1);
		}

		public async Task PostUpdatesAsync(IReadOnlyList<DiscordEmbedBuilder> embeds)
		{
			await this._semaphore.WaitAsync().ConfigureAwait(false);
			try
			{
				for (var i = 0; i < embeds.Count; i++)
				{
					var embed = embeds[i];
					this._logger.LogTrace("Posting update to {Channel} - {@Embed}", this._channel, embed);
					await this._channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
				}
			}
			finally
			{
				this._semaphore.Release();
			}
		}
	}
}