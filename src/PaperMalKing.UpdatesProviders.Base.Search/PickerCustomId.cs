// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Runtime.InteropServices;

namespace PaperMalKing.UpdatesProviders.Base.Search;

[StructLayout(LayoutKind.Auto)]
internal readonly record struct PickerCustomId(Guid SearchId, PickerAction Action)
{
	public const string Prefix = "search:";
	private const int SearchIdLength = 32;

	public static bool HasPrefix(string value) => value.StartsWith(Prefix, StringComparison.Ordinal);

	public static string Create(Guid searchId, PickerAction action)
	{
		var actionValue = action switch
		{
			PickerAction.Pick => "pick",
			PickerAction.Previous => "previous",
			PickerAction.Page => "page",
			PickerAction.Next => "next",
			PickerAction.Cancel => "cancel",
			_ => throw new ArgumentOutOfRangeException(nameof(action)),
		};
		return $"{Prefix}{searchId:N}:{actionValue}";
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

		if (!Guid.TryParseExact(remainder[..separatorIndex], "N", out var searchId))
		{
			return false;
		}

		var action = remainder[(separatorIndex + 1)..] switch
		{
			"pick" => PickerAction.Pick,
			"previous" => PickerAction.Previous,
			"page" => PickerAction.Page,
			"next" => PickerAction.Next,
			"cancel" => PickerAction.Cancel,
			_ => (PickerAction?)null,
		};
		if (!action.HasValue)
		{
			return false;
		}

		customId = new(searchId, action.Value);
		return true;
	}
}
