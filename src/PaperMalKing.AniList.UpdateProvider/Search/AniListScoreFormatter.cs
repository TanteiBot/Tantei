// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static class AniListScoreFormatter
{
	[SuppressMessage("Major Code Smell", "S109:Magic numbers should not be used", Justification = "Obvious from usage")]
	public static string? Format(ushort? averageScore, ScoreFormat scoreFormat)
	{
		if (averageScore is not { } score)
		{
			return null;
		}

		return scoreFormat switch
		{
			ScoreFormat.POINT_100 => $"{score}/100",
			ScoreFormat.POINT_10_DECIMAL or ScoreFormat.POINT_10 => $"{(score / 10d).ToString(CultureInfo.InvariantCulture)}/10",
			ScoreFormat.POINT_5 => $"{Math.Round(score / 20d, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture)}/5",
			ScoreFormat.POINT_3 => score switch
			{
				<= 33 => ":(",
				<= 66 => ":|",
				_ => ":)",
			},
			_ => $"{score}/100",
		};
	}
}
