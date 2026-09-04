// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal static class SearchMessages
{
	public const string MissingPermissions = "I can't post an embed in this channel. I need the Embed Links and Send Messages permissions here.";
	public const string QueryWithoutLettersOrDigits = "That query contains no letters or digits. Search for a title instead.";
	public const string PostFailed = "I couldn't post that result. Check my channel permissions and try again.";
	public const string Unavailable = "This search is no longer available. Run the command again.";
	public const string Unexpected = "Something went wrong with this search. Run the command again.";
	public const string Cancelled = "Search cancelled.";
	public const string IdledOut = "This search idled out. Run the command again.";
	public const string Expired = "This search has expired. Run the command again.";
	private const int QuotedQueryLimit = 100;

	public static string QueryTooShort(int minimum) =>
		$"Search with at least {minimum.ToString(CultureInfo.InvariantCulture)} character{(minimum == 1 ? "" : "s")}.";

	public static string Busy(string providerDisplayName) => $"{providerDisplayName} is busy — try again in a moment.";

	public static string Failed(string providerDisplayName) => $"Searching {providerDisplayName} failed. Try again in a moment.";

	public static string NoResults(string query) => $"No results for {Quote(query)}";

	public static string TypeFilterEmpty(string query) => $"No result for {Quote(query)} matches the selected type.";

	private static string Quote(string query) => SearchText.Truncate(query, QuotedQueryLimit);
}
