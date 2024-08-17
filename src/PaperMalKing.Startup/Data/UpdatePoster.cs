// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
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

	public Task PreparePostingUpdatesAsync() => this._semaphore.WaitAsync();

	public int FinishPostingUpdates() => this._semaphore.Release();

	public Task<DiscordMessage> PostUpdateAsync(DiscordEmbed embed)
	{
		this._logger.PostingUpdate(this._channel, embed);
		return this._channel.SendMessageAsync(embed: embed);
	}

	public void Dispose()
	{
		this._semaphore.Dispose();
	}
}