// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using EntityFramework.Exceptions.Sqlite;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Startup.Options;
using PaperMalKing.Startup.Web.Tokens;

namespace Tantei.Tests;

public sealed class DiscordTokenRefreshServiceTests
{
	private const ulong UserId = 123UL;

	private const string AccessToken = "access";

	private const string RefreshToken = "refresh";

	private const string RotatedAccessToken = "rotated-access";

	private const string RotatedRefreshToken = "rotated-refresh";

	private const int ExpiresInSeconds = 604800;

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
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
		using (var db = factory.CreateDbContext())
		{
			await db.Database.EnsureCreatedAsync();
		}

		return (new(factory, provider.GetRequiredService<IDataProtectionProvider>(), timeProvider, NullLogger<DiscordOAuthTokenStore>.Instance), factory,
			connection);
	}

	private static IOptions<DiscordOptions> CreateDiscordOptions() =>
		Options.Create(new DiscordOptions
		{
			Token = "token",
			ClientId = "client-id",
			ClientSecret = "client-secret",
			Activities = [],
		});

	private static DiscordTokenRefreshService CreateService(DiscordOAuthTokenStore store, FakeTimeProvider timeProvider, FakeHttpMessageHandler handler) =>
		new(store, CreateDiscordOptions(), timeProvider, NullLogger<DiscordTokenRefreshService>.Instance, handler);

	[Test]
	public async Task TransientHttpFailureLeavesStoredTokenIntactAndReturnsNull()
	{
		var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(start);
		var (store, _, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		store.Save(UserId, AccessToken, RefreshToken, start.AddSeconds(1));

		using var handler = new FakeHttpMessageHandler(_ => new(HttpStatusCode.TooManyRequests));
		using var service = CreateService(store, timeProvider, handler);

		var result = await service.GetValidAccessTokenAsync(UserId, cancellationToken);

		await Assert.That(result).IsNull();
		await Assert.That(handler.CallCount).IsEqualTo(1);
		var stored = store.Get(UserId);
		await Assert.That(stored).IsNotNull();
		await Assert.That(stored!.AccessToken).IsEqualTo(AccessToken);
	}

	[Test]
	public async Task InvalidGrantDeletesTheRowAndReturnsNull()
	{
		var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(start);
		var (store, _, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		store.Save(UserId, AccessToken, RefreshToken, start.AddSeconds(1));

		using var handler = new FakeHttpMessageHandler(_ => new(HttpStatusCode.BadRequest)
		{
			Content = JsonContent.Create(new { error = "invalid_grant" }),
		});
		using var service = CreateService(store, timeProvider, handler);

		var result = await service.GetValidAccessTokenAsync(UserId, cancellationToken);

		await Assert.That(result).IsNull();
		await Assert.That(store.Get(UserId)).IsNull();
	}

	[Test]
	public async Task SuccessfulRefreshStoresRotatedPairAndReturnsNewAccessToken()
	{
		var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(start);
		var (store, _, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		store.Save(UserId, AccessToken, RefreshToken, start.AddSeconds(1));

		using var handler = new FakeHttpMessageHandler(_ => new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(new
			{
				access_token = RotatedAccessToken,
				refresh_token = RotatedRefreshToken,
				expires_in = ExpiresInSeconds,
			}),
		});
		using var service = CreateService(store, timeProvider, handler);

		var result = await service.GetValidAccessTokenAsync(UserId, cancellationToken);

		await Assert.That(result).IsEqualTo(RotatedAccessToken);
		var stored = store.Get(UserId);
		await Assert.That(stored!.AccessToken).IsEqualTo(RotatedAccessToken);
		await Assert.That(stored.RefreshToken).IsEqualTo(RotatedRefreshToken);
	}

	[Test]
	public async Task ConcurrentCallersForSameUserProduceExactlyOneHttpCall()
	{
		var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(start);
		var (store, _, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		store.Save(UserId, AccessToken, RefreshToken, start.AddSeconds(1));

		using var handler = new FakeHttpMessageHandler(_ => new(HttpStatusCode.OK)
		{
			Content = JsonContent.Create(new
			{
				access_token = RotatedAccessToken,
				refresh_token = RotatedRefreshToken,
				expires_in = ExpiresInSeconds,
			}),
		});
		using var service = CreateService(store, timeProvider, handler);

		var first = service.GetValidAccessTokenAsync(UserId, cancellationToken);
		var second = service.GetValidAccessTokenAsync(UserId, cancellationToken);
		var results = await Task.WhenAll(first, second);

		await Assert.That(handler.CallCount).IsEqualTo(1);
		await Assert.That(results[0]).IsEqualTo(RotatedAccessToken);
		await Assert.That(results[1]).IsEqualTo(RotatedAccessToken);
	}

	[Test]
	public async Task TokenWithinExpiryMarginIsReturnedWithoutAnyHttpCall()
	{
		var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
		var timeProvider = new FakeTimeProvider(start);
		var (store, _, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		store.Save(UserId, AccessToken, RefreshToken, start.AddHours(1));

		using var handler = new FakeHttpMessageHandler(_ => throw new InvalidOperationException("HTTP should not have been called"));
		using var service = CreateService(store, timeProvider, handler);

		var result = await service.GetValidAccessTokenAsync(UserId, cancellationToken);

		await Assert.That(result).IsEqualTo(AccessToken);
		await Assert.That(handler.CallCount).IsEqualTo(0);
	}

	private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
	{
		public DateTimeOffset Now { get; set; } = now;

		public override DateTimeOffset GetUtcNow() => this.Now;
	}

	private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
	{
		private int _callCount;

		public int CallCount => this._callCount;

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Interlocked.Increment(ref this._callCount);
			return Task.FromResult(respond(request));
		}
	}
}
