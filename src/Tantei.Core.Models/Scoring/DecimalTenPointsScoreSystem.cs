// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Globalization;

namespace Tantei.Core.Models.Scoring;

public sealed class DecimalTenPointsScoreSystem : BaseScoreSystem<float>
{
	private static readonly Dictionary<float, DecimalTenPointsScoreSystem> Scores = Generate();

	private DecimalTenPointsScoreSystem(float userScore, string? display) : base(userScore, display)
	{ }

	private static Dictionary<float, DecimalTenPointsScoreSystem> Generate()
	{
		var dict = new Dictionary<float, DecimalTenPointsScoreSystem>(101);
		Span<char> buffer = stackalloc char[3];
		buffer[1] = '.';
		for (byte i = 0; i < 10; i++)
		{
			i.TryFormat(buffer[..1], out var _);
			for (byte j = 1; j < 10; j++)
			{
				j.TryFormat(buffer.Slice(2, 1), out var _);
				var value = float.Parse(buffer);
				dict[value] = new(value, buffer.ToString());
			}
		}

		for (float i = 1; i < 11; i++)
		{
			dict[i] = new(i, i.ToString(CultureInfo.InvariantCulture));
		}

		dict[0f] = new(0f, null);
		dict.TrimExcess();
		return dict;
	}

	public static DecimalTenPointsScoreSystem Create(float userScore)
	{
		return Scores.TryGetValue(userScore, out var result) ? result : throw new ArgumentOutOfRangeException(nameof(userScore));
	}
}