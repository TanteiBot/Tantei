// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Collections;
using System.Globalization;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class SearchLogScope : IReadOnlyList<KeyValuePair<string, object?>>
{
	private readonly KeyValuePair<string, object?>[] _fields;

	public int Count => this._fields.Length;

	public KeyValuePair<string, object?> this[int index] => this._fields[index];

	private SearchLogScope(KeyValuePair<string, object?>[] fields)
	{
		this._fields = fields;
	}

	public static SearchLogScope ForSearch(string searchId, PickerSearchContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		return new(
		[
			new("SearchId", searchId),
			new("Query", context.Query),
			new("MediaKind", context.MediaKind),
			new("TypeFilter", context.MediaTypeFilter),
			new("DiscordUserId", context.DiscordUserId),
			new("DiscordDisplayName", context.RequesterDisplayName),
			new("GuildId", context.GuildId),
			new("ChannelId", context.ChannelId),
		]);
	}

	public static SearchLogScope ForInteraction(string searchId, ulong discordUserId, string discordDisplayName, ulong? guildId, ulong? channelId) =>
		new(
		[
			new("SearchId", searchId),
			new("DiscordUserId", discordUserId),
			new("DiscordDisplayName", discordDisplayName),
			new("GuildId", guildId),
			new("ChannelId", channelId),
		]);

	public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, object?>>)this._fields).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => this._fields.GetEnumerator();

	public override string ToString() =>
		string.Join(' ', this._fields.Select(static field => field.Key + '=' + Convert.ToString(field.Value, CultureInfo.InvariantCulture)));
}
