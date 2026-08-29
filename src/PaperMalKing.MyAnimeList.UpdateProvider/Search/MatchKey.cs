// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using System.Text;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class MatchKey : IEquatable<MatchKey>
{
	private const int CombiningKatakanaHiraganaVoicedSoundMark = 0x3099;
	private const int CombiningKatakanaHiraganaSemiVoicedSoundMark = 0x309A;

	private MatchKey(string value)
	{
		this.Value = value;
	}

	public string Value { get; }

	public bool IsEmpty => this.Value.Length == 0;

	public static MatchKey Create(string value)
	{
		ArgumentNullException.ThrowIfNull(value);
		var decomposed = value.Normalize(NormalizationForm.FormKC)
			.ToUpperInvariant()
			.Normalize(NormalizationForm.FormD);
		var retainedMarks = new StringBuilder(decomposed.Length);
		foreach (var rune in decomposed.EnumerateRunes())
		{
			var category = Rune.GetUnicodeCategory(rune);
			if (category == UnicodeCategory.NonSpacingMark && rune.Value is not CombiningKatakanaHiraganaVoicedSoundMark and not CombiningKatakanaHiraganaSemiVoicedSoundMark)
			{
				continue;
			}

			retainedMarks.Append(rune);
		}

		var recomposed = retainedMarks.ToString().Normalize(NormalizationForm.FormC);
		var key = new StringBuilder(recomposed.Length);
		foreach (var rune in recomposed.EnumerateRunes())
		{
			if (!Rune.IsLetterOrDigit(rune))
			{
				continue;
			}

			key.Append(rune);
		}

		return new(key.ToString());
	}

	public bool Contains(MatchKey other) => this.Value.Contains(other.Value, StringComparison.Ordinal);

	public bool Equals(MatchKey? other) => other is not null && this.Value.Equals(other.Value, StringComparison.Ordinal);

	public override bool Equals(object? obj) => obj is MatchKey other && this.Equals(other);

	public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(this.Value);
}
