// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using EntityFramework.Exceptions.Sqlite;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
		var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync();

		var services = new ServiceCollection();
		services.AddDbContextFactory<DatabaseContext>(o => o.UseSqlite(connection).UseExceptionProcessor());
		services.AddDataProtection();
		var provider = services.BuildServiceProvider();

		var factory = provider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
		await using (var db = await factory.CreateDbContextAsync())
		{
			await db.Database.EnsureCreatedAsync();
		}

		return (new(factory, provider.GetRequiredService<IDataProtectionProvider>(), timeProvider, NullLogger<DiscordOAuthTokenStore>.Instance), factory,
			connection);
	}

	[Test]
	public async Task SavedTokenRoundTrips()
	{
		var expiresAt = new DateTimeOffset(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);
		var (store, _, connection) = await CreateStoreAsync(TimeProvider.System);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		await store.SaveAsync(UserId, AccessToken, RefreshToken, expiresAt, cancellationToken);
		var stored = await store.GetAsync(UserId, cancellationToken);

		await Assert.That(stored).IsNotNull();
		await Assert.That(stored!.AccessToken).IsEqualTo(AccessToken);
		await Assert.That(stored.RefreshToken).IsEqualTo(RefreshToken);
		await Assert.That(stored.ExpiresAt).IsEqualTo(expiresAt);
	}

	[Test]
	public async Task TokensAreNotStoredInPlaintext()
	{
		var (store, factory, connection) = await CreateStoreAsync(TimeProvider.System);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		await store.SaveAsync(UserId, AccessToken, RefreshToken, DateTimeOffset.UnixEpoch, cancellationToken);

		await using var db = await factory.CreateDbContextAsync(cancellationToken);
		var row = db.DiscordOAuthTokens.Single();
		await Assert.That(row.AccessToken).IsNotEqualTo(AccessToken);
		await Assert.That(row.RefreshToken).IsNotEqualTo(RefreshToken);
	}

	[Test]
	public async Task SavingTwiceOverwritesTheExistingRow()
	{
		var (store, factory, connection) = await CreateStoreAsync(TimeProvider.System);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		await store.SaveAsync(UserId, "first", "firstRefresh", DateTimeOffset.UnixEpoch, cancellationToken);
		await store.SaveAsync(UserId, "second", "secondRefresh", DateTimeOffset.UnixEpoch, cancellationToken);

		await using var db = await factory.CreateDbContextAsync(cancellationToken);
		await Assert.That(db.DiscordOAuthTokens.Count()).IsEqualTo(1);
		var stored = await store.GetAsync(UserId, cancellationToken);
		await Assert.That(stored!.AccessToken).IsEqualTo("second");
	}

	[Test]
	public async Task DeletedTokenIsGone()
	{
		var (store, _, connection) = await CreateStoreAsync(TimeProvider.System);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		await store.SaveAsync(UserId, AccessToken, RefreshToken, DateTimeOffset.UnixEpoch, cancellationToken);
		await store.DeleteAsync(UserId, cancellationToken);

		await Assert.That(await store.GetAsync(UserId, cancellationToken)).IsNull();
	}

	[Test]
	public async Task PruneRemovesOnlyRowsUnusedSinceThreshold()
	{
		var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(start);
		var (store, _, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		await store.SaveAsync(UserId, AccessToken, RefreshToken, DateTimeOffset.UnixEpoch, cancellationToken);
		timeProvider.Now = start.AddDays(LaterInDays);
		await store.SaveAsync(OtherUserId, AccessToken, RefreshToken, DateTimeOffset.UnixEpoch, cancellationToken);

		var removed = await store.PruneUnusedSinceAsync(start.AddDays(StaleAfterDays), cancellationToken);

		await Assert.That(removed).IsEqualTo(1);
		await Assert.That(await store.GetAsync(UserId, cancellationToken)).IsNull();
		await Assert.That(await store.GetAsync(OtherUserId, cancellationToken)).IsNotNull();
	}

	private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
	{
		public DateTimeOffset Now { get; set; } = now;

		public override DateTimeOffset GetUtcNow() => this.Now;
	}
}
