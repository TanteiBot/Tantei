// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using PaperMalKing.Database.Models.AniList;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.Database.Models.Shikimori;

namespace PaperMalKing.UpdateProviders.Base.Tests;

public sealed class UserFeaturesFlagsTest
{
	[Theory]
	[InlineData(typeof(MalUserFeatures))]
	[InlineData(typeof(ShikiUserFeatures))]
	[InlineData(typeof(AniListUserFeatures))]
	public void UserFeaturesTypeMustHaveFlagsAttribute(Type featuresType)
	{
		Assert.Contains(typeof(FlagsAttribute), featuresType.GetCustomAttributes(typeof(FlagsAttribute), true).Select(x=>x.GetType()));
	}
}