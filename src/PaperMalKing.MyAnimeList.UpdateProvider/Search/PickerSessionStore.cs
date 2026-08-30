// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Caching.Memory;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class PickerSessionStore(IMemoryCache _cache)
{
	private const string SessionPrefix = "mal-search-session:";
	private const string TerminalPrefix = "mal-search-terminal:";
	private static readonly TimeSpan TerminalMarkerLifetime = TimeSpan.FromMinutes(1);

	public void Add(PickerSession session, TimeSpan lifetime)
	{
		ArgumentNullException.ThrowIfNull(session);
		_cache.Set(SessionKey(session.SearchId), session, new MemoryCacheEntryOptions()
			.SetAbsoluteExpiration(lifetime)
			.RegisterPostEvictionCallback(static (_, value, _, _) => (value as PickerSession)?.OnEvicted()));
	}

	public PickerSessionLookup Find(string searchId)
	{
		if (_cache.TryGetValue(SessionKey(searchId), out PickerSession? session))
		{
			return new(PickerLookup.Active, session);
		}

		var kind = _cache.TryGetValue(TerminalKey(searchId), out _) ? PickerLookup.Terminal : PickerLookup.Absent;
		return new(kind, Session: null);
	}

	public void End(PickerSession session)
	{
		_cache.Set(TerminalKey(session.SearchId), value: true, absoluteExpirationRelativeToNow: TerminalMarkerLifetime);
		_cache.Remove(SessionKey(session.SearchId));
	}

	private static string SessionKey(string searchId) => SessionPrefix + searchId;

	private static string TerminalKey(string searchId) => TerminalPrefix + searchId;
}
