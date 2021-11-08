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

using System.Globalization;

namespace Tantei.Core.Models.Scoring;

public sealed class TenPointsScoreSystem : BaseScoreSystem<byte>
{
	private static readonly Dictionary<byte, TenPointsScoreSystem> Scores = Generate();

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
		if (Scores.TryGetValue(userScore, out var result))
		{
			return result;
		}

		throw new ArgumentOutOfRangeException(nameof(userScore), "Score must be withing range of [0;10]");
	}

	private TenPointsScoreSystem(byte userScore, string? display) : base(userScore, display)
	{ }
}