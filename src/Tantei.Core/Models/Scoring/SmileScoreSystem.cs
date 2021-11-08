// Tantei.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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

	public static SmileScoreSystem Create(byte userScore)
	{
		if (Scores.TryGetValue(userScore, out var result))
		{
			return result;
		}
		throw new ArgumentOutOfRangeException(nameof(userScore), "Score must be within [0;3] range");
	}

	private SmileScoreSystem(byte userScore, string? display) : base(userScore, display)
	{ }
}