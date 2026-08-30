// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Buffers;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal readonly record struct PickerCustomId(string SearchId, PickerAction Action)
{
	public const string Prefix = "mal:search:";
	private const int SearchIdLength = 32;
	private static readonly SearchValues<char> SearchIdCharacters = SearchValues.Create("0123456789abcdef");

	public static bool HasPrefix(string value) => value.StartsWith(Prefix, StringComparison.Ordinal);

	public static string Create(string searchId, PickerAction action)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(searchId);
		var actionValue = action switch
		{
			PickerAction.Pick => "pick",
			PickerAction.Previous => "previous",
			PickerAction.Page => "page",
			PickerAction.Next => "next",
			PickerAction.Cancel => "cancel",
			_ => throw new ArgumentOutOfRangeException(nameof(action)),
		};
		return $"{Prefix}{searchId}:{actionValue}";
	}

	public static bool TryParse(string value, out PickerCustomId customId)
	{
		customId = default;
		if (!HasPrefix(value))
		{
			return false;
		}

		var remainder = value.AsSpan(Prefix.Length);
		var separatorIndex = remainder.IndexOf(':');
		if (separatorIndex != SearchIdLength || remainder[(separatorIndex + 1)..].Contains(':'))
		{
			return false;
		}

		var searchId = remainder[..separatorIndex];
		if (!searchId.ContainsAnyExcept(SearchIdCharacters))
		{
			var action = remainder[(separatorIndex + 1)..] switch
			{
				"pick" => PickerAction.Pick,
				"previous" => PickerAction.Previous,
				"page" => PickerAction.Page,
				"next" => PickerAction.Next,
				"cancel" => PickerAction.Cancel,
				_ => (PickerAction?)null,
			};
			if (action.HasValue)
			{
				customId = new(searchId.ToString(), action.Value);
				return true;
			}
		}

		return false;
	}
}
