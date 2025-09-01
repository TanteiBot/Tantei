// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using PaperMalKing.MyAnimeList.Wrapper.Abstractions;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class FieldsToRequestTests
{
	[Test]
	public async Task MangaFieldsToRequestAndAnimeFieldsToRequestHaveSameStartingValues()
	{
		const int enumStart = 4;
		var aftr = Enum.GetNames<AnimeFieldsToRequest>();
		var mftr = Enum.GetNames<MangaFieldsToRequest>();
		await Assert.That(aftr[..enumStart]).IsEqualTo(mftr[..enumStart], new SequenceEqualityComparer());
	}

	[Test]
	public async Task FieldsToRequestEnumsHaveByteAsUnderlyingType()
	{
		static async Task Check(Type t)
		{
			await Assert.That(Enum.GetUnderlyingType(t)).IsEqualTo(typeof(byte));
		}

		await Check(typeof(MangaFieldsToRequest));
		await Check(typeof(AnimeFieldsToRequest));
	}

	private sealed class SequenceEqualityComparer : IEqualityComparer<string[]>
	{
		public bool Equals(string[]? x, string[]? y)
		{
			switch (x)
			{
				case null when y is null:

				case [] when y is []:
					return true;

				default:
					return x.SequenceEqual(y);
			}
		}

		public int GetHashCode(string[] obj) => obj.GetHashCode();
	}
}