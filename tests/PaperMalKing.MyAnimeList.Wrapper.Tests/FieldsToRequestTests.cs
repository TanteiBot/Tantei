// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using TUnit.Assertions.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class FieldsToRequestTests
{
	[Test]
	public async Task MangaFieldsToRequestAndAnimeFieldsToRequestHaveSameStartingValues()
	{
		const int enumStart = 4;
		var aftr = Enum.GetNames<AnimeFieldsToRequest>();
		var mftr = Enum.GetNames<MangaFieldsToRequest>();
		await Assert.That(aftr[..enumStart]).IsEquivalentTo(mftr[..enumStart], CollectionOrdering.Matching);
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
}