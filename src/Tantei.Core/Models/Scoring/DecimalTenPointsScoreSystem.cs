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

public sealed class DecimalTenPointsScoreSystem : BaseScoreSystem<float>
{
	private static readonly Dictionary<float, DecimalTenPointsScoreSystem> Scores = Generate();

	private static Dictionary<float, DecimalTenPointsScoreSystem> Generate()
	{
		var dict = new Dictionary<float, DecimalTenPointsScoreSystem>(101);
		Span<char> buffer = stackalloc char[3];
		buffer[1] = '.';
		for (byte i = 0; i < 10; i++)
		{
			i.TryFormat(buffer.Slice(0, 1), out var _);
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
		if (Scores.TryGetValue(userScore, out var result))
		{
			return result;
		}

		throw new ArgumentOutOfRangeException(nameof(userScore));
	}

	private DecimalTenPointsScoreSystem(float userScore, string? display) : base(userScore, display)
	{ }
}