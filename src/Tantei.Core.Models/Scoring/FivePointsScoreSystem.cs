// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Globalization;

namespace Tantei.Core.Models.Scoring;

public sealed class FivePointsScoreSystem : BaseScoreSystem<byte>
{
	private static readonly Dictionary<byte, FivePointsScoreSystem> Scores = Generate();

	private FivePointsScoreSystem(byte userScore, string? displayValue) : base(userScore, displayValue)
	{ }

	private static Dictionary<byte, FivePointsScoreSystem> Generate()
	{
		var dict = new Dictionary<byte, FivePointsScoreSystem>(6);
		for (byte i = 1; i < 6; i++)
		{
			dict[i] = new(i, $"{i.ToString(CultureInfo.InvariantCulture)}/5");
		}

		dict[0] = new(0, null);
		dict.TrimExcess();
		return dict;
	}

	public static FivePointsScoreSystem Create(byte userScore)
	{
		return Scores.TryGetValue(userScore, out var result)
			? result
			: throw new ArgumentOutOfRangeException(nameof(userScore), "Score must be within [1;5] range");
	}
}