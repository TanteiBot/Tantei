// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.Database;
using PaperMalKing.Startup.Web.Tokens;

namespace Tantei.Tests;

public sealed class DiscordOAuthTokenStoreTests
{
	private const ulong UserId = 123UL;

	private const ulong OtherUserId = 456UL;

	private const string AccessToken = "access";

	private const string RefreshToken = "refresh";

	private const int StaleAfterDays = 30;

	private const int LaterInDays = 40;

	private static async Task<(DiscordOAuthTokenStore Store, IDbContextFactory<DatabaseContext> Factory, SqliteConnection Connection)> CreateStoreAsync(
		TimeProvider timeProvider)
	{
		var (factory, connection, dataProtection) = await SqliteInMemoryDatabase.CreateAsync(addDataProtection: true);
		return (new(factory, dataProtection!, timeProvider, NullLogger<DiscordOAuthTokenStore>.Instance), factory, connection);
	}

	[Test]
	public async Task SavedTokenRoundTrips()
	{
		var expiresAt = new DateTimeOffset(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);
		var (store, _, connection) = await CreateStoreAsync(TimeProvider.System);
		await using var ownedConnection = connection;

		store.Save(UserId, AccessToken, RefreshToken, expiresAt);
		var stored = store.Get(UserId);

		await Assert.That(stored).IsNotNull();
		await Assert.That(stored!.AccessToken).IsEqualTo(AccessToken);
		await Assert.That(stored.RefreshToken).IsEqualTo(RefreshToken);
		await Assert.That(stored.ExpiresAt).IsEqualTo(expiresAt);
	}

	[Test]
	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public async Task TokensAreNotStoredInPlaintext()
	{
		var (store, factory, connection) = await CreateStoreAsync(TimeProvider.System);
		await using var ownedConnection = connection;

		store.Save(UserId, AccessToken, RefreshToken, DateTimeOffset.UnixEpoch);

		using var db = factory.CreateDbContext();
		var row = db.DiscordOAuthTokens.Single();
		await Assert.That(row.AccessToken).IsNotEqualTo(AccessToken);
		await Assert.That(row.RefreshToken).IsNotEqualTo(RefreshToken);
	}

	[Test]
	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public async Task SavingTwiceOverwritesTheExistingRow()
	{
		var (store, factory, connection) = await CreateStoreAsync(TimeProvider.System);
		await using var ownedConnection = connection;

		store.Save(UserId, "first", "firstRefresh", DateTimeOffset.UnixEpoch);
		store.Save(UserId, "second", "secondRefresh", DateTimeOffset.UnixEpoch);

		using var db = factory.CreateDbContext();
		await Assert.That(db.DiscordOAuthTokens.Count()).IsEqualTo(1);
		var stored = store.Get(UserId);
		await Assert.That(stored!.AccessToken).IsEqualTo("second");
	}

	[Test]
	public async Task DeletedTokenIsGone()
	{
		var (store, _, connection) = await CreateStoreAsync(TimeProvider.System);
		await using var ownedConnection = connection;

		store.Save(UserId, AccessToken, RefreshToken, DateTimeOffset.UnixEpoch);
		store.Delete(UserId);

		await Assert.That(store.Get(UserId)).IsNull();
	}

	[Test]
	public async Task PruneRemovesOnlyRowsUnusedSinceThreshold()
	{
		var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(start);
		var (store, _, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;

		store.Save(UserId, AccessToken, RefreshToken, DateTimeOffset.UnixEpoch);
		timeProvider.SetUtcNow(start.AddDays(LaterInDays));
		store.Save(OtherUserId, AccessToken, RefreshToken, DateTimeOffset.UnixEpoch);

		var removed = store.PruneUnusedSince(start.AddDays(StaleAfterDays));

		await Assert.That(removed).IsEqualTo(1);
		await Assert.That(store.Get(UserId)).IsNull();
		await Assert.That(store.Get(OtherUserId)).IsNotNull();
	}
}
