// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;

namespace PaperMalKing.Startup.Web.Tokens;

public sealed record StoredDiscordToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public sealed class DiscordOAuthTokenStore(IDbContextFactory<DatabaseContext> _dbContextFactory,
										   IDataProtectionProvider dataProtectionProvider,
										   TimeProvider _timeProvider)
{
	private const string ProtectorPurpose = "Tantei.DiscordOAuthToken.v1";

	private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);

	public async Task SaveAsync(ulong discordUserId, string accessToken, string refreshToken, DateTimeOffset expiresAt, CancellationToken cancellationToken)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
		var existing = db.DiscordOAuthTokens.FirstOrDefault(x => x.DiscordUserId == discordUserId);
		var now = _timeProvider.GetUtcNow();
		if (existing is null)
		{
			db.DiscordOAuthTokens.Add(new()
			{
				DiscordUserId = discordUserId,
				AccessToken = this._protector.Protect(accessToken),
				RefreshToken = this._protector.Protect(refreshToken),
				ExpiresAt = expiresAt,
				LastUsedAt = now,
			});
		}
		else
		{
			existing.AccessToken = this._protector.Protect(accessToken);
			existing.RefreshToken = this._protector.Protect(refreshToken);
			existing.ExpiresAt = expiresAt;
			existing.LastUsedAt = now;
		}

		await db.SaveChangesAsync(cancellationToken);
	}

	public async Task<StoredDiscordToken?> GetAsync(ulong discordUserId, CancellationToken cancellationToken)
	{
		StoredDiscordToken? result;
		await using (var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken))
		{
			var row = db.DiscordOAuthTokens.AsNoTracking().FirstOrDefault(x => x.DiscordUserId == discordUserId);
			if (row is null)
			{
				return null;
			}

			try
			{
				result = new(this._protector.Unprotect(row.AccessToken), this._protector.Unprotect(row.RefreshToken), row.ExpiresAt);
			}
			catch (System.Security.Cryptography.CryptographicException)
			{
				result = null;
			}
		}

		if (result is null)
		{
			await this.DeleteAsync(discordUserId, cancellationToken);
		}

		return result;
	}

	public async Task DeleteAsync(ulong discordUserId, CancellationToken cancellationToken)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
		db.DiscordOAuthTokens.Where(x => x.DiscordUserId == discordUserId).ExecuteDelete();
	}

	public async Task<int> PruneUnusedSinceAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
		return db.DiscordOAuthTokens.Where(x => x.LastUsedAt < threshold).ExecuteDelete();
	}
}
