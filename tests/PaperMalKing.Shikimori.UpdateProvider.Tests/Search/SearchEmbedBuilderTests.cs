// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Common.Enums;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.UpdateProvider.Search;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests.Search;

public sealed class SearchEmbedBuilderTests
{
	private const string RequesterDisplayName = "nodan";
	private const string AvatarUrl = "https://cdn.discordapp.com/avatars/1/abc.png";
	private const string AnimeUrl = "https://shikimori.io/animes/1";
	private const string MangaUrl = "https://shikimori.io/mangas/2";

	[Test]
	public async Task AnimeEmbedLinksTitleAndRendersFeatureGatedMetadata()
	{
		var media = new AnimeSearchMedia
		{
			Id = 1UL,
			Name = "Monster",
			Kind = "tv",
			Score = 8.7f,
			Status = "released",
			Url = AnimeUrl,
			Genres = [new() { Name = "Thriller", RussianName = "Триллер" }],
			Studios = [new() { Id = 1U, Name = "Madhouse" }],
			Description = "A brilliant surgeon hunts a former patient.",
		};
		const ShikiUserFeatures features = ShikiUserFeatures.MediaFormat | ShikiUserFeatures.MediaStatus |
										   ShikiUserFeatures.Genres | ShikiUserFeatures.Studio | ShikiUserFeatures.Description;

		var embed = SearchEmbedBuilder.Build(media, ListEntryType.Anime, features, useRussian: false, RequesterDisplayName, AvatarUrl);

		await Assert.That(embed.Title).StartsWith("Monster");
		await Assert.That(embed.Url).IsEqualTo(AnimeUrl);
		await Assert.That(embed.Footer.Text).IsEqualTo("Shikimori");
		await Assert.That(embed.Author.Name).IsEqualTo("Requested by nodan");
		var fieldNames = embed.Fields.Select(static field => field.Name).ToArray();
		await Assert.That(fieldNames).Contains("Score");
		await Assert.That(fieldNames).Contains("Studio");
		await Assert.That(fieldNames).Contains("Genres");
		await Assert.That(fieldNames).Contains("Description");
	}

	[Test]
	public async Task DisabledFeaturesDropTheirFields()
	{
		var media = new AnimeSearchMedia
		{
			Id = 1UL,
			Name = "Monster",
			Kind = "tv",
			Status = "released",
			Url = AnimeUrl,
			Genres = [new() { Name = "Thriller", RussianName = "Триллер" }],
			Studios = [new() { Id = 1U, Name = "Madhouse" }],
		};

		var embed = SearchEmbedBuilder.Build(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, RequesterDisplayName, AvatarUrl);

		var fieldNames = embed.Fields.Select(static field => field.Name).ToArray();
		await Assert.That(fieldNames).DoesNotContain("Genres");
		await Assert.That(fieldNames).DoesNotContain("Studio");
	}

	[Test]
	public async Task MangaEmbedRendersPublishers()
	{
		var media = new MangaSearchMedia
		{
			Id = 2UL,
			Name = "Berserk",
			Kind = "manga",
			Status = "ongoing",
			Url = MangaUrl,
			Publishers = [new() { Id = 1U, Name = "Hakusensha" }],
		};

		var embed = SearchEmbedBuilder.Build(media, ListEntryType.Manga, ShikiUserFeatures.Publisher, useRussian: false, RequesterDisplayName, AvatarUrl);

		await Assert.That(embed.Url).IsEqualTo(MangaUrl);
		await Assert.That(embed.Fields.Select(static field => field.Name)).Contains("Publisher");
	}

	[Test]
	public async Task TitleHonorsRussianPreference()
	{
		var media = new MangaSearchMedia
		{
			Id = 2UL,
			Name = "Berserk",
			RussianName = "Берсерк",
			Kind = "manga",
			Status = "ongoing",
			Url = MangaUrl,
		};

		var embed = SearchEmbedBuilder.Build(media, ListEntryType.Manga, ShikiUserFeatures.Russian, useRussian: true, RequesterDisplayName, AvatarUrl);

		await Assert.That(embed.Title).StartsWith("Берсерк");
	}
}
