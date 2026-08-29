// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json;
using Microsoft.Extensions.Logging;
using PaperMalKing.Startup.Web.Tokens;

namespace PaperMalKing.Startup.Web.Guilds;

public sealed class UserGuildsProvider(UserGuildsCache _userGuildsCache,
									   DiscordTokenRefreshService _tokenRefreshService,
									   DiscordUserGuildsClient _guildsClient,
									   ILogger<UserGuildsProvider> _logger) : IUserGuildsProvider
{
	public async Task<IReadOnlyList<DiscordPartialGuild>?> GetGuildsAsync(ulong discordUserId, CancellationToken cancellationToken)
	{
		if (_userGuildsCache.TryGet(discordUserId, out var cached))
		{
			return cached;
		}

		var accessToken = await _tokenRefreshService.GetValidAccessTokenAsync(discordUserId, cancellationToken);
		if (accessToken is null)
		{
			return null;
		}

		try
		{
			var guilds = await _guildsClient.GetGuildsAsync(accessToken, cancellationToken);
			_userGuildsCache.Set(discordUserId, guilds);
			return guilds;
		}
		catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException or JsonException) && !cancellationToken.IsCancellationRequested)
		{
			_logger.FailedToRefetchDiscordGuilds(ex, discordUserId);
			return null;
		}
	}
}
