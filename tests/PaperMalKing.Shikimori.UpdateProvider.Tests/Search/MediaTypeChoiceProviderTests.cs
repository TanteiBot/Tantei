// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests.Search;

public sealed class MediaTypeChoiceProviderTests
{
	[Test]
	public async Task AnimeKindProviderOffersEveryKindExceptUnknownWithLabels()
	{
		var choices = await MediaTypeChoiceProvider<AnimeKind>.CreateChoicesAsync();
		var byValue = choices.ToDictionary(static choice => (string)choice.Value, static choice => choice.Name, StringComparer.Ordinal);

		await Assert.That(byValue.Keys).IsEquivalentTo([
			nameof(AnimeKind.Tv),
			nameof(AnimeKind.Movie),
			nameof(AnimeKind.Ova),
			nameof(AnimeKind.Ona),
			nameof(AnimeKind.Special),
			nameof(AnimeKind.TvSpecial),
			nameof(AnimeKind.Music),
			nameof(AnimeKind.Pv),
			nameof(AnimeKind.Cm),
		]);
		await Assert.That(byValue).DoesNotContainKey(nameof(AnimeKind.Unknown));
		await Assert.That(byValue[nameof(AnimeKind.TvSpecial)]).IsEqualTo("Tv special");
	}

	[Test]
	public async Task MangaKindProviderOffersEveryKindExceptUnknownWithLabels()
	{
		var choices = await MediaTypeChoiceProvider<MangaKind>.CreateChoicesAsync();
		var byValue = choices.ToDictionary(static choice => (string)choice.Value, static choice => choice.Name, StringComparer.Ordinal);

		await Assert.That(byValue.Keys).IsEquivalentTo([
			nameof(MangaKind.Manga),
			nameof(MangaKind.Manhwa),
			nameof(MangaKind.Manhua),
			nameof(MangaKind.LightNovel),
			nameof(MangaKind.Novel),
			nameof(MangaKind.OneShot),
			nameof(MangaKind.Doujin),
		]);
		await Assert.That(byValue).DoesNotContainKey(nameof(MangaKind.Unknown));
		await Assert.That(byValue[nameof(MangaKind.LightNovel)]).IsEqualTo("Light novel");
	}

	[Test]
	public async Task EveryChoiceValueParsesBackToItsKind()
	{
		foreach (var choice in await MediaTypeChoiceProvider<AnimeKind>.CreateChoicesAsync())
		{
			await Assert.That(MediaTypeChoiceProvider<AnimeKind>.Parse((string)choice.Value)).IsNotNull();
		}

		await Assert.That(MediaTypeChoiceProvider<MangaKind>.Parse(value: null)).IsNull();
		await Assert.That(MediaTypeChoiceProvider<AnimeKind>.Parse("not-a-kind")).IsNull();
	}
}
