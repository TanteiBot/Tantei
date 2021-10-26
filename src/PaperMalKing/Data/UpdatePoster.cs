#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
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

using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.Data;

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