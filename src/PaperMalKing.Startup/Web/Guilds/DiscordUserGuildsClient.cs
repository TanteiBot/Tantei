// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace PaperMalKing.Startup.Web.Guilds;

public sealed class DiscordUserGuildsClient(HttpClient _httpClient)
{
	public const string HttpClientName = "DiscordUserGuilds";

	public async Task<IReadOnlyList<DiscordPartialGuild>> GetGuildsAsync(string accessToken, CancellationToken cancellationToken)
	{
		using var request = new HttpRequestMessage(HttpMethod.Get, "users/@me/guilds");
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
		using var response = await _httpClient.SendAsync(request, cancellationToken);
		response.EnsureSuccessStatusCode();
		var payload = await response.Content.ReadFromJsonAsync<IReadOnlyList<PartialGuildPayload>>(cancellationToken) ?? [];
		return [.. payload.Select(Map)];
	}

	private static DiscordPartialGuild Map(PartialGuildPayload payload)
	{
		var id = ulong.Parse(payload.Id, CultureInfo.InvariantCulture);
		var permissions = ulong.Parse(payload.Permissions, CultureInfo.InvariantCulture);
		var iconUrl = payload.Icon is null ? null : $"https://cdn.discordapp.com/icons/{payload.Id}/{payload.Icon}.png";
		return new(id, payload.Name, iconUrl, permissions);
	}

	private sealed record PartialGuildPayload(
		[property: JsonPropertyName("id")] string Id,
		[property: JsonPropertyName("name")] string Name,
		[property: JsonPropertyName("icon")] string? Icon,
		[property: JsonPropertyName("permissions")] string Permissions);
}
