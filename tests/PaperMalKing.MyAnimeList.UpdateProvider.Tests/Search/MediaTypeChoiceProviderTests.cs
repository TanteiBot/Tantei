// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class MediaTypeChoiceProviderTests
{
	[Test]
	public async Task AnimeChoicesDropUnknownAndCarryHumanizedLabels()
	{
		var choices = await MediaTypeChoiceProvider<AnimeMediaType>.CreateChoicesAsync();
		var byValue = choices.ToDictionary(static choice => (string)choice.Value, static choice => choice.Name, StringComparer.Ordinal);

		await Assert.That(byValue).DoesNotContainKey(nameof(AnimeMediaType.Unknown));
		await Assert.That(byValue.Count).IsEqualTo(Enum.GetValues<AnimeMediaType>().Length - 1);
		await Assert.That(byValue[nameof(AnimeMediaType.TvSpecial)]).IsEqualTo("TV Special");
		await Assert.That(byValue[nameof(AnimeMediaType.TV)]).IsEqualTo("TV");
	}

	[Test]
	public async Task MangaChoicesCarryHumanizedLabels()
	{
		var choices = await MediaTypeChoiceProvider<MangaMediaType>.CreateChoicesAsync();
		var byValue = choices.ToDictionary(static choice => (string)choice.Value, static choice => choice.Name, StringComparer.Ordinal);

		await Assert.That(byValue).DoesNotContainKey(nameof(MangaMediaType.Unknown));
		await Assert.That(byValue[nameof(MangaMediaType.OneShot)]).IsEqualTo("One-shot");
	}

	[Test]
	public async Task EveryChoiceValueParsesBackToItsMediaType()
	{
		var choices = await MediaTypeChoiceProvider<AnimeMediaType>.CreateChoicesAsync();

		foreach (var choice in choices)
		{
			await Assert.That(MediaTypeChoiceProvider<AnimeMediaType>.Parse((string)choice.Value)).IsNotNull();
		}

		await Assert.That(MediaTypeChoiceProvider<AnimeMediaType>.Parse(value: null)).IsNull();
		await Assert.That(MediaTypeChoiceProvider<AnimeMediaType>.Parse("not-a-media-type")).IsNull();
	}
}
