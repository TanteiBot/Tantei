// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Net;
using DSharpPlus;
using EntityFramework.Exceptions.Sqlite;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Startup.Options;
using PaperMalKing.Startup.Web.Guilds;
using PaperMalKing.Startup.Web.Tokens;

namespace Tantei.Tests;

public sealed class UserGuildsProviderTests
{
	private const ulong UserId = 42UL;

	private const ulong CachedGuildId = 100UL;

	private const ulong FetchedGuildId = 200UL;

	private const string GuildsPayload = """[{"id":"200","name":"Fetched","icon":null,"permissions":"32"}]""";

	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(7);

	private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
	{
		public int CallCount { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			this.CallCount++;
			return Task.FromResult(respond(request));
		}
	}

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	private static async Task<(DiscordOAuthTokenStore Store, SqliteConnection Connection)> CreateStoreAsync(TimeProvider timeProvider)
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

		return (new(factory, provider.GetRequiredService<IDataProtectionProvider>(), timeProvider, NullLogger<DiscordOAuthTokenStore>.Instance), connection);
	}

	private static DiscordTokenRefreshService CreateTokenRefreshService(DiscordOAuthTokenStore store, TimeProvider timeProvider) =>
		new(store,
			Options.Create(new DiscordOptions { Token = "token", ClientId = "client-id", ClientSecret = "client-secret", Activities = [], }),
			timeProvider,
			NullLogger<DiscordTokenRefreshService>.Instance);

	private static UserGuildsProvider CreateProvider(UserGuildsCache cache, DiscordTokenRefreshService tokenRefreshService, HttpClient httpClient)
		=> new(cache, tokenRefreshService, new(httpClient), NullLogger<UserGuildsProvider>.Instance);

	private static HttpClient CreateHttpClient(FakeHttpMessageHandler handler)
		=> new(handler) { BaseAddress = new(PaperMalKing.Startup.Web.DiscordApiConstants.BaseUrl), };

	[Test]
	public async Task ReturnsCachedGuildsWithoutCallingDiscord()
	{
		var timeProvider = new FakeTimeProvider(Start);
		var (store, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var cache = new UserGuildsCache(memoryCache);
		cache.Set(UserId, [new(CachedGuildId, "Cached", null, Permissions.ManageGuild),]);

		using var handler = new FakeHttpMessageHandler(_ => new(HttpStatusCode.OK));
		using var httpClient = CreateHttpClient(handler);
		using var tokenRefreshService = CreateTokenRefreshService(store, timeProvider);

		var guilds = await CreateProvider(cache, tokenRefreshService, httpClient).GetGuildsAsync(UserId, cancellationToken);

		await Assert.That(guilds).IsNotNull();
		await Assert.That(guilds!.Select(g => g.Id)).IsEquivalentTo([CachedGuildId,]);
		await Assert.That(handler.CallCount).IsEqualTo(0);
	}

	[Test]
	public async Task RefetchesFromDiscordWhenTheCacheIsColdButTheTokenIsStillGood()
	{
		var timeProvider = new FakeTimeProvider(Start);
		var (store, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		store.Save(UserId, "access", "refresh", Start + TokenLifetime);

		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var cache = new UserGuildsCache(memoryCache);

		using var handler = new FakeHttpMessageHandler(_ => new(HttpStatusCode.OK) { Content = new StringContent(GuildsPayload), });
		using var httpClient = CreateHttpClient(handler);
		using var tokenRefreshService = CreateTokenRefreshService(store, timeProvider);

		var guilds = await CreateProvider(cache, tokenRefreshService, httpClient).GetGuildsAsync(UserId, cancellationToken);

		await Assert.That(guilds).IsNotNull();
		await Assert.That(guilds!.Select(g => g.Id)).IsEquivalentTo([FetchedGuildId,]);
		await Assert.That(cache.TryGet(UserId, out _)).IsTrue();
	}

	[Test]
	public async Task ReturnsNullWhenTheCacheIsColdAndNoTokenIsStored()
	{
		var timeProvider = new FakeTimeProvider(Start);
		var (store, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		using var handler = new FakeHttpMessageHandler(_ => new(HttpStatusCode.OK));
		using var httpClient = CreateHttpClient(handler);
		using var tokenRefreshService = CreateTokenRefreshService(store, timeProvider);

		var guilds = await CreateProvider(new(memoryCache), tokenRefreshService, httpClient).GetGuildsAsync(UserId, cancellationToken);

		await Assert.That(guilds).IsNull();
	}

	[Test]
	public async Task ReturnsNullWhenDiscordCannotBeReached()
	{
		var timeProvider = new FakeTimeProvider(Start);
		var (store, connection) = await CreateStoreAsync(timeProvider);
		await using var ownedConnection = connection;
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		store.Save(UserId, "access", "refresh", Start + TokenLifetime);

		using var memoryCache = new MemoryCache(new MemoryCacheOptions());
		using var handler = new FakeHttpMessageHandler(_ => throw new HttpRequestException("boom"));
		using var httpClient = CreateHttpClient(handler);
		using var tokenRefreshService = CreateTokenRefreshService(store, timeProvider);

		var guilds = await CreateProvider(new(memoryCache), tokenRefreshService, httpClient).GetGuildsAsync(UserId, cancellationToken);

		await Assert.That(guilds).IsNull();
	}
}
