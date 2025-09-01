// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using PaperMalKing.Database.Models.AniList;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.Database.Models.Shikimori;

namespace PaperMalKing.UpdateProviders.Base.Tests;

public class UserFeaturesTypesTests
{
	[Test]
	[Arguments(typeof(MalUserFeatures))]
	[Arguments(typeof(ShikiUserFeatures))]
	[Arguments(typeof(AniListUserFeatures))]
	public async Task FeaturesHaveUlongAsUnderlyingType(Type featureType)
	{
		await Assert.That(Enum.GetUnderlyingType(featureType)).IsEqualTo(typeof(ulong));
	}
}