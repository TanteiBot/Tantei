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
		if (Scores.TryGetValue(userScore, out var result))
		{
			return result;
		}

		throw new ArgumentOutOfRangeException(nameof(userScore), "Argument must be within [0;100] range");
	}
}