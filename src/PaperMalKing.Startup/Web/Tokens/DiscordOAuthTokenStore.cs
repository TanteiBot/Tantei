// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;

namespace PaperMalKing.Startup.Web.Tokens;

public sealed record StoredDiscordToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

public sealed class DiscordOAuthTokenStore(IDbContextFactory<DatabaseContext> _dbContextFactory,
										   IDataProtectionProvider dataProtectionProvider,
										   TimeProvider _timeProvider,
										   ILogger<DiscordOAuthTokenStore> _logger)
{
	private const string ProtectorPurpose = "Tantei.DiscordOAuthToken.v1";

	private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);

	public void Save(ulong discordUserId, string accessToken, string refreshToken, DateTimeOffset expiresAt)
	{
		var token = new ProtectedToken(this._protector.Protect(accessToken),
									   this._protector.Protect(refreshToken),
									   expiresAt,
									   _timeProvider.GetUtcNow());

		if (this.TryUpdate(discordUserId, token) ||
			this.TryInsert(discordUserId, token))
		{
			return;
		}

		if (!this.TryUpdate(discordUserId, token))
		{
			_logger.FailedToPersistRotatedDiscordToken(discordUserId);
		}
	}

	private bool TryUpdate(ulong discordUserId, ProtectedToken token)
	{
		using var db = _dbContextFactory.CreateDbContext();
		var existing = db.DiscordOAuthTokens.FirstOrDefault(x => x.DiscordUserId == discordUserId);
		if (existing is null)
		{
			return false;
		}

		existing.AccessToken = token.AccessToken;
		existing.RefreshToken = token.RefreshToken;
		existing.ExpiresAt = token.ExpiresAt;
		existing.LastUsedAt = token.LastUsedAt;
		db.SaveChanges();
		return true;
	}

	private bool TryInsert(ulong discordUserId, ProtectedToken token)
	{
		using var db = _dbContextFactory.CreateDbContext();
		db.DiscordOAuthTokens.Add(new()
		{
			DiscordUserId = discordUserId,
			AccessToken = token.AccessToken,
			RefreshToken = token.RefreshToken,
			ExpiresAt = token.ExpiresAt,
			LastUsedAt = token.LastUsedAt,
		});

		try
		{
			db.SaveChanges();
			return true;
		}
		catch (UniqueConstraintException)
		{
			return false;
		}
	}

	public StoredDiscordToken? Get(ulong discordUserId)
	{
		StoredDiscordToken? result;
		using (var db = _dbContextFactory.CreateDbContext())
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
			this.Delete(discordUserId);
		}

		return result;
	}

	public void Delete(ulong discordUserId)
	{
		using var db = _dbContextFactory.CreateDbContext();
		db.DiscordOAuthTokens.Where(x => x.DiscordUserId == discordUserId).ExecuteDelete();
	}

	public int PruneUnusedSince(DateTimeOffset threshold)
	{
		using var db = _dbContextFactory.CreateDbContext();
		return db.DiscordOAuthTokens.Where(x => x.LastUsedAt < threshold).ExecuteDelete();
	}

	private readonly record struct ProtectedToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, DateTimeOffset LastUsedAt);
}
