// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Database.Models.AniList;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.Database.Models.Shikimori;

namespace PaperMalKing.UpdateProviders.Base.Tests;

public sealed class UserFeaturesFlagsTest
{
	[Test]
	[Arguments(typeof(MalUserFeatures))]
	[Arguments(typeof(ShikiUserFeatures))]
	[Arguments(typeof(AniListUserFeatures))]
	public async Task UserFeaturesTypeMustHaveFlagsAttribute(Type featuresType)
	{
		await Assert.That(featuresType.GetCustomAttributes(typeof(FlagsAttribute), inherit: true).Select(static x => x.GetType()))
					.Contains(typeof(FlagsAttribute));
	}
}