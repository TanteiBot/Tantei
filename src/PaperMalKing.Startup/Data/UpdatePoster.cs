// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Data;

internal sealed class UpdatePoster(ILogger<UpdatePoster> _logger, DiscordChannel _channel) : IDisposable
{
	private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();

	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public Task PreparePostingUpdatesAsync() => this._semaphore.WaitAsync();

	public int FinishPostingUpdates() => this._semaphore.Release();

	[SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "Memory stream is not async")]
	public async Task<DiscordMessage> PostUpdateAsync(UpdateContents update)
	{
		var embed = update.EmbedBuilder.Build();
		_logger.PostingUpdate(_channel, embed);

		var dmb = new DiscordMessageBuilder
		{
			Embed = embed,
		};

		List<IDisposable> disposables = [];

		try
		{
			foreach (var updateFile in update.Files)
			{
				var stream = MemoryStreamManager.GetStream();
				stream.Write(updateFile.Content, 0, updateFile.Content.Length);
				stream.Seek(0, SeekOrigin.Begin);
				disposables.Add(stream);
				dmb.AddFile(updateFile.Filename, stream);
			}

			return await _channel.SendMessageAsync(dmb);
		}
		finally
		{
			foreach (var disposable in disposables)
			{
				disposable.Dispose();
			}
		}
	}

	public void Dispose()
	{
		this._semaphore.Dispose();
	}
}