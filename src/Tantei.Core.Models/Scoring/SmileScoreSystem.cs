// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Scoring;

public sealed class SmileScoreSystem : BaseScoreSystem<byte>
{
	private static readonly Dictionary<byte, SmileScoreSystem> Scores = new()
	{
		[0] = new(0, null),
		[1] = new(1, ":("),
		[2] = new(2, ":|"),
		[3] = new(3, ":)")
	};

	private SmileScoreSystem(byte userScore, string? display) : base(userScore, display)
	{ }

	public static SmileScoreSystem Create(byte userScore)
	{
		return Scores.TryGetValue(userScore, out var result)
			? result
			: throw new ArgumentOutOfRangeException(nameof(userScore), "Score must be within [0;3] range");
	}
}