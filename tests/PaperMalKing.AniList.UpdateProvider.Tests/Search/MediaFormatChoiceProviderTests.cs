// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.AniList.UpdateProvider.Search;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Tests.Search;

public sealed class MediaFormatChoiceProviderTests
{
	[Test]
	public async Task AnimeProviderOffersExactlyItsFormatSubsetWithLabels()
	{
		var choices = await AnimeMediaFormatChoiceProvider.CreateChoicesAsync();
		var byValue = choices.ToDictionary(static choice => (string)choice.Value, static choice => choice.Name, StringComparer.Ordinal);

		await Assert.That(byValue.Keys).IsEquivalentTo([
			nameof(MediaFormat.TV),
			nameof(MediaFormat.TvShort),
			nameof(MediaFormat.Movie),
			nameof(MediaFormat.Special),
			nameof(MediaFormat.OVA),
			nameof(MediaFormat.ONA),
			nameof(MediaFormat.Music),
		]);
		await Assert.That(byValue[nameof(MediaFormat.TvShort)]).IsEqualTo("TV Short");
		await Assert.That(byValue[nameof(MediaFormat.TV)]).IsEqualTo("TV");
		await Assert.That(byValue).DoesNotContainKey(nameof(MediaFormat.Manga));
	}

	[Test]
	public async Task MangaProviderOffersExactlyItsFormatSubsetWithLabels()
	{
		var choices = await MangaMediaFormatChoiceProvider.CreateChoicesAsync();
		var byValue = choices.ToDictionary(static choice => (string)choice.Value, static choice => choice.Name, StringComparer.Ordinal);

		await Assert.That(byValue.Keys).IsEquivalentTo([
			nameof(MediaFormat.Manga),
			nameof(MediaFormat.Novel),
			nameof(MediaFormat.OneShot),
		]);
		await Assert.That(byValue[nameof(MediaFormat.Manga)]).IsEqualTo("Manga");
		await Assert.That(byValue).DoesNotContainKey(nameof(MediaFormat.TV));
	}

	[Test]
	public async Task ParseReturnsNullForMissingOrUnknownFormat()
	{
		await Assert.That(AnimeMediaFormatChoiceProvider.Parse(value: null)).IsNull();
		await Assert.That(AnimeMediaFormatChoiceProvider.Parse("not-a-format")).IsNull();
	}

	[Test]
	public async Task MangaParseReturnsNullForMissingOrUnknownFormat()
	{
		await Assert.That(MangaMediaFormatChoiceProvider.Parse(value: null)).IsNull();
		await Assert.That(MangaMediaFormatChoiceProvider.Parse("not-a-format")).IsNull();
	}
}
