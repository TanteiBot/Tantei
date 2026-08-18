// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Startup.Options;

namespace PaperMalKing.Startup.Web.Tokens;

public sealed class DiscordTokenRefreshService(DiscordOAuthTokenStore _tokenStore,
											   IOptions<DiscordOptions> _discordOptions,
											   TimeProvider _timeProvider,
											   ILogger<DiscordTokenRefreshService> _logger,
											   HttpMessageHandler? httpMessageHandler = null) : IDisposable
{
	private const string InvalidGrantError = "invalid_grant";

	private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

	private readonly ConcurrentDictionary<ulong, SemaphoreSlim> _locks = new();

	private readonly HttpClient _httpClient =
		new(httpMessageHandler ?? new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) })
		{
			BaseAddress = new("https://discord.com/api/v10/"),
		};

	private static async Task<string?> TryReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
	{
		try
		{
			var payload = await response.Content.ReadFromJsonAsync<ErrorPayload>(cancellationToken);
			return payload?.Error;
		}
		catch (JsonException)
		{
			return null;
		}
	}

	public async Task<string?> GetValidAccessTokenAsync(ulong discordUserId, CancellationToken cancellationToken)
	{
		var stored = await _tokenStore.GetAsync(discordUserId, cancellationToken);
		if (stored is null)
		{
			return null;
		}

		if (stored.ExpiresAt - _timeProvider.GetUtcNow() > RefreshMargin)
		{
			return stored.AccessToken;
		}

		var semaphore = this._locks.GetOrAdd(discordUserId, static _ => new(1, 1));
		await semaphore.WaitAsync(cancellationToken);
		try
		{
			var current = await _tokenStore.GetAsync(discordUserId, cancellationToken);
			if (current is null)
			{
				return null;
			}

			if (current.ExpiresAt - _timeProvider.GetUtcNow() > RefreshMargin)
			{
				return current.AccessToken;
			}

			return await this.RefreshAsync(discordUserId, current.RefreshToken, cancellationToken);
		}
		finally
		{
			semaphore.Release();
		}
	}

	private async Task<string?> RefreshAsync(ulong discordUserId, string refreshToken, CancellationToken cancellationToken)
	{
		using var content = new FormUrlEncodedContent(new Dictionary<string, string>(StringComparer.Ordinal)
		{
			["client_id"] = _discordOptions.Value.ClientId,
			["client_secret"] = _discordOptions.Value.ClientSecret,
			["grant_type"] = "refresh_token",
			["refresh_token"] = refreshToken,
		});

		using var response = await this._httpClient.PostAsync(new Uri("oauth2/token", UriKind.Relative), content, cancellationToken);
		if (!response.IsSuccessStatusCode)
		{
			if (response.StatusCode == HttpStatusCode.BadRequest)
			{
				var error = await TryReadErrorAsync(response, cancellationToken);
				if (string.Equals(error, InvalidGrantError, StringComparison.Ordinal))
				{
					_logger.DiscardingUnusableDiscordToken(discordUserId, error);
					await _tokenStore.DeleteAsync(discordUserId, cancellationToken);
					return null;
				}
			}

			_logger.TransientDiscordTokenRefreshFailure(discordUserId, response.StatusCode);
			return null;
		}

		var payload = await response.Content.ReadFromJsonAsync<TokenPayload>(cancellationToken);
		if (payload is null)
		{
			_logger.TransientDiscordTokenRefreshFailure(discordUserId, response.StatusCode);
			return null;
		}

		var expiresAt = _timeProvider.GetUtcNow().AddSeconds(payload.ExpiresIn);
		await _tokenStore.SaveAsync(discordUserId, payload.AccessToken, payload.RefreshToken, expiresAt, cancellationToken);
		return payload.AccessToken;
	}

	public void Dispose()
	{
		this._httpClient.Dispose();
		foreach (var semaphore in this._locks.Values)
		{
			semaphore.Dispose();
		}
	}

	private sealed record TokenPayload(
		[property: JsonPropertyName("access_token")] string AccessToken,
		[property: JsonPropertyName("refresh_token")] string RefreshToken,
		[property: JsonPropertyName("expires_in")] int ExpiresIn);

	private sealed record ErrorPayload([property: JsonPropertyName("error")] string? Error);
}
