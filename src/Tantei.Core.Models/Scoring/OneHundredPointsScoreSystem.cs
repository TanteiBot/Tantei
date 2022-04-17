// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Globalization;

namespace Tantei.Core.Models.Scoring;

public sealed class OneHundredPointsScoreSystem : BaseScoreSystem<byte>
{
	private static readonly Dictionary<byte, OneHundredPointsScoreSystem> Scores = Generate();

	private OneHundredPointsScoreSystem(byte userScore, string? displayString) : base(userScore, displayString)
	{ }

	private static Dictionary<byte, OneHundredPointsScoreSystem> Generate()
	{
		var dict = new Dictionary<byte, OneHundredPointsScoreSystem>(101);
		for (byte i = 1; i < 101; i++)
		{
			dict[i] = new(i, $"{i.ToString(CultureInfo.InvariantCulture)}/100");
		}

		dict[0] = new(0, null);
		dict.TrimExcess();
		return dict;
	}

	public static OneHundredPointsScoreSystem Create(byte userScore)
	{
		return Scores.TryGetValue(userScore, out var result)
			? result
			: throw new ArgumentOutOfRangeException(nameof(userScore), "Argument must be within [0;100] range");
	}
}