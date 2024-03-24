// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using PaperMalKing.Database.Models.AniList;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.Database.Models.Shikimori;

namespace PaperMalKing.UpdateProviders.Base.Tests;

public class UserFeaturesTypesTests
{
	[Theory]
	[InlineData(typeof(MalUserFeatures))]
	[InlineData(typeof(ShikiUserFeatures))]
	[InlineData(typeof(AniListUserFeatures))]
	public void FeaturesHaveUlongAsUnderlyingType(Type featureType)
	{
		Assert.StrictEqual(typeof(ulong), Enum.GetUnderlyingType(featureType));
	}
}