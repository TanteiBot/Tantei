// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.Startup.Data;

internal sealed class UpdatePoster : IDisposable
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
		await this._semaphore.WaitAsync();
		try
		{
			for (var i = 0; i < embeds.Count; i++)
			{
				var embed = embeds[i].Build();
				this._logger.PostingUpdate(this._channel, embed);
				await this._channel.SendMessageAsync(embed: embed);
			}
		}
		finally
		{
			this._semaphore.Release();
		}
	}

	public void Dispose()
	{
		var semaphore = this._semaphore;
		semaphore.Dispose();
	}
}