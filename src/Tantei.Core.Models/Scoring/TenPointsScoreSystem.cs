// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Globalization;

namespace Tantei.Core.Models.Scoring;

public sealed class TenPointsScoreSystem : BaseScoreSystem<byte>
{
	private static readonly Dictionary<byte, TenPointsScoreSystem> Scores = Generate();

	private TenPointsScoreSystem(byte userScore, string? display) : base(userScore, display)
	{ }

	private static Dictionary<byte, TenPointsScoreSystem> Generate()
	{
		var dict = new Dictionary<byte, TenPointsScoreSystem>(11);
		for (byte i = 1; i < 11; i++)
		{
			dict[i] = new(i, i.ToString(CultureInfo.InvariantCulture));
		}

		dict[0] = new(0, null);
		dict.TrimExcess();
		return dict;
	}

	public static TenPointsScoreSystem Create(byte userScore)
	{
		return Scores.TryGetValue(userScore, out var result)
			? result
			: throw new ArgumentOutOfRangeException(nameof(userScore), "Score must be withing range of [0;10]");
	}
}