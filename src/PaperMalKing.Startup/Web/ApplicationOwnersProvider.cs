// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;

namespace PaperMalKing.Startup.Web;

public interface IApplicationOwnersSource
{
	Task<IReadOnlyCollection<ulong>> GetOwnerIdsAsync(CancellationToken cancellationToken);
}

public sealed class DiscordApplicationOwnersSource(DiscordClient _discordClient) : IApplicationOwnersSource
{
	public async Task<IReadOnlyCollection<ulong>> GetOwnerIdsAsync(CancellationToken cancellationToken)
	{
		var application = await _discordClient.GetCurrentApplicationAsync();
		return [.. application.Owners.Select(static o => o.Id)];
	}
}

public sealed class ApplicationOwnersProvider(IApplicationOwnersSource _source, TimeProvider _timeProvider) : IDisposable
{
	private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(1);

	private readonly SemaphoreSlim _semaphore = new(1, 1);

	private IReadOnlyCollection<ulong> _ownerIds = [];

	private DateTimeOffset _fetchedAt = DateTimeOffset.MinValue;

	public void Dispose() => this._semaphore.Dispose();

	public async Task<bool> IsOwnerAsync(ulong discordUserId, CancellationToken cancellationToken)
	{
		var owners = await this.GetOwnersAsync(cancellationToken);
		return owners.Contains(discordUserId);
	}

	private async Task<IReadOnlyCollection<ulong>> GetOwnersAsync(CancellationToken cancellationToken)
	{
		if (_timeProvider.GetUtcNow() - this._fetchedAt < CacheLifetime)
		{
			return this._ownerIds;
		}

		await this._semaphore.WaitAsync(cancellationToken);
#pragma warning disable ERP022
		try
		{
			if (_timeProvider.GetUtcNow() - this._fetchedAt < CacheLifetime)
			{
				return this._ownerIds;
			}

			this._ownerIds = await _source.GetOwnerIdsAsync(cancellationToken);
			this._fetchedAt = _timeProvider.GetUtcNow();
		}
#pragma warning disable CA1031
		catch (Exception)
#pragma warning restore CA1031
		{
			this._ownerIds = [];
		}
		finally
		{
			this._semaphore.Release();
		}
#pragma warning restore ERP022

		return this._ownerIds;
	}
}
