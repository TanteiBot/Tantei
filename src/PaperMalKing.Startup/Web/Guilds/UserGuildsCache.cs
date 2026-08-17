// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;

namespace PaperMalKing.Startup.Web.Guilds;

public sealed record DiscordPartialGuild(ulong Id, string Name, string? IconUrl, ulong Permissions);

public sealed class UserGuildsCache(IMemoryCache _memoryCache)
{
	private const int MaxCachedGuilds = 200;

	private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

	public void Set(ulong discordUserId, IReadOnlyList<DiscordPartialGuild> guilds)
		=> _memoryCache.Set(Key(discordUserId), (IReadOnlyList<DiscordPartialGuild>)[.. guilds.Take(MaxCachedGuilds)], Lifetime);

	public bool TryGet(ulong discordUserId, [NotNullWhen(true)] out IReadOnlyList<DiscordPartialGuild>? guilds)
		=> _memoryCache.TryGetValue(Key(discordUserId), out guilds);

	public void Evict(ulong discordUserId) => _memoryCache.Remove(Key(discordUserId));

	private static string Key(ulong discordUserId) => $"user-guilds:{discordUserId.ToString(CultureInfo.InvariantCulture)}";
}
