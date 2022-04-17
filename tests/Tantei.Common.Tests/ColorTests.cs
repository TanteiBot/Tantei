// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Common.Tests;

public class ColorTests
{
	public static IEnumerable<object[]> Data =>
		new List<object[]>
		{
			new object[] { Color.AniList.Green.Value, 123, 213, 85 },
			new object[] { Color.AniList.Orange.Value, 247, 154, 99 },
			new object[] { Color.AniList.Red.Value, 232, 93, 117 },
			new object[] { Color.AniList.Peach.Value, 250, 122, 122 },
			new object[] { Color.AniList.Blue.Value, 61, 180, 242 },
			new object[] { Color.MyAnimeList.Green.Value, 45, 176, 57 },
			new object[] { Color.MyAnimeList.Blue.Value, 38, 68, 143 },
			new object[] { Color.MyAnimeList.Yellow.Value, 249, 212, 87 },
			new object[] { Color.MyAnimeList.Red.Value, 161, 47, 49 },
			new object[] { Color.MyAnimeList.Grey.Value, 195, 195, 195 },
			new object[] { Color.Black.Value, 35, 39, 42 },
			new object[] { Color.Shikimori.Green.Value, 65, 149, 65 },
			new object[] { Color.Shikimori.Red.Value, 252, 87, 94 },
			new object[] { Color.Shikimori.Grey.Value, 123, 128, 132 },
			new object[] { Color.Shikimori.Blue.Value, 23, 96, 147 },
			new object[] { 0x5865F2, 88, 101, 242 },
			new object[] { 0x57F287, 87, 242, 135 },
			new object[] { 0xFEE75C, 254, 231, 92 },
			new object[] { 0xEB459E, 235, 69, 158 },
			new object[] { 0xED4245, 237, 66, 69 }
		};

	[Theory]
	[MemberData(nameof(Data))]
	public void ColorConstructorFromIntWorksCorrect(int value, byte r, byte g, byte b)
	{
		var colorFromInt = new Color(value);
		Assert.Equal(value, colorFromInt.Value);
		Assert.Equal(r, colorFromInt.R);
		Assert.Equal(g, colorFromInt.G);
		Assert.Equal(b, colorFromInt.B);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public void ColorConstructorFromRgbBytesWorksCorrect(int value, byte r, byte g, byte b)
	{
		var colorFromBytes = new Color(r, g, b);
		Assert.Equal(value, colorFromBytes.Value);
		Assert.Equal(r, colorFromBytes.R);
		Assert.Equal(g, colorFromBytes.G);
		Assert.Equal(b, colorFromBytes.B);
	}
}