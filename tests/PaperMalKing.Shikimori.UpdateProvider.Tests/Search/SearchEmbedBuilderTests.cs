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
	private const string MonsterTitle = "Monster";
	private const string BerserkTitle = "Berserk";
	private const string ThrillerGenre = "Thriller";
	private const string GenresField = "Genres";

	[Test]
	public async Task AnimeEmbedLinksTitleAndRendersFeatureGatedMetadata()
	{
		var media = new AnimeSearchMedia
		{
			Id = 1UL,
			Name = MonsterTitle,
			Kind = "tv",
			Score = 8.7f,
			Status = "released",
			Url = AnimeUrl,
			Genres = [new() { Name = ThrillerGenre, RussianName = "Триллер" }],
			Studios = [new() { Id = 1U, Name = "Madhouse" }],
			Description = "A brilliant surgeon hunts a former patient.",
		};
		const ShikiUserFeatures features = ShikiUserFeatures.MediaFormat | ShikiUserFeatures.MediaStatus |
										   ShikiUserFeatures.Genres | ShikiUserFeatures.Studio | ShikiUserFeatures.Description;

		var embed = SearchEmbedBuilder.Build(media, ListEntryType.Anime, features, useRussian: false, RequesterDisplayName, AvatarUrl);

		await Assert.That(embed.Title).StartsWith(MonsterTitle);
		await Assert.That(embed.Url).IsEqualTo(AnimeUrl);
		await Assert.That(embed.Footer.Text).IsEqualTo("Shikimori");
		await Assert.That(embed.Author.Name).IsEqualTo("Requested by nodan");
		var fieldNames = embed.Fields.Select(static field => field.Name).ToArray();
		await Assert.That(fieldNames).Contains("Community score");
		await Assert.That(fieldNames).Contains("Studio");
		await Assert.That(fieldNames).Contains(GenresField);
		await Assert.That(fieldNames).Contains("Description");
	}

	[Test]
	public async Task DisabledFeaturesDropTheirFields()
	{
		var media = new AnimeSearchMedia
		{
			Id = 1UL,
			Name = MonsterTitle,
			Kind = "tv",
			Status = "released",
			Url = AnimeUrl,
			Genres = [new() { Name = ThrillerGenre, RussianName = "Триллер" }],
			Studios = [new() { Id = 1U, Name = "Madhouse" }],
		};

		var embed = SearchEmbedBuilder.Build(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, RequesterDisplayName, AvatarUrl);

		var fieldNames = embed.Fields.Select(static field => field.Name).ToArray();
		await Assert.That(fieldNames).DoesNotContain(GenresField);
		await Assert.That(fieldNames).DoesNotContain("Studio");
	}

	[Test]
	public async Task MangaEmbedRendersPublishers()
	{
		var media = new MangaSearchMedia
		{
			Id = 2UL,
			Name = BerserkTitle,
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
	public async Task SearchDefaultAnimeRendersFullMediaInfoWithNativeNames()
	{
		var media = new AnimeSearchMedia
		{
			Id = 1UL,
			Name = MonsterTitle,
			RussianName = "Монстр",
			Kind = "tv",
			Score = 8.7f,
			Status = "released",
			Url = AnimeUrl,
			Genres = [new() { Name = ThrillerGenre, RussianName = "Триллер" }],
			Studios = [new() { Id = 1U, Name = "Madhouse" }],
			Description = "A brilliant surgeon hunts a former patient.",
			PersonRoles =
			[
				new()
				{
					Name = ["Director"],
					RussianName = ["Режиссёр"],
					Person = new() { Id = 5U, Name = "Masayuki Kojima", RussianName = "Масаюки Кодзима" },
				},
			],
		};

		var embed = SearchEmbedBuilder.Build(media, ListEntryType.Anime, ShikiUserFeatures.SearchDefault, useRussian: false, RequesterDisplayName, AvatarUrl);

		var fields = embed.Fields.ToDictionary(static field => field.Name, static field => field.Value, StringComparer.Ordinal);
		await Assert.That(embed.Title).StartsWith(MonsterTitle);
		await Assert.That(embed.Title).DoesNotContain("Монстр");
		await Assert.That(fields.Keys).Contains("Studio");
		await Assert.That(fields.Keys).Contains(GenresField);
		await Assert.That(fields.Keys).Contains("Description");
		await Assert.That(fields.Keys).Contains("Director");
		await Assert.That(fields["Director"]).Contains("Masayuki Kojima");
		await Assert.That(fields[GenresField]).IsEqualTo(ThrillerGenre);
	}

	[Test]
	public async Task SearchDefaultMangaRendersPublisherAndMangakaWithNativeNames()
	{
		var media = new MangaSearchMedia
		{
			Id = 2UL,
			Name = BerserkTitle,
			RussianName = "Берсерк",
			Kind = "manga",
			Status = "ongoing",
			Url = MangaUrl,
			Publishers = [new() { Id = 1U, Name = "Hakusensha" }],
			PersonRoles =
			[
				new()
				{
					Name = ["Story & Art"],
					RussianName = ["История и рисунки"],
					Person = new() { Id = 7U, Name = "Kentarou Miura", RussianName = "Кэнтаро Миура", IsMangaka = true },
				},
			],
		};

		var embed = SearchEmbedBuilder.Build(media, ListEntryType.Manga, ShikiUserFeatures.SearchDefault, useRussian: false, RequesterDisplayName, AvatarUrl);

		var fields = embed.Fields.ToDictionary(static field => field.Name, static field => field.Value, StringComparer.Ordinal);
		await Assert.That(embed.Title).StartsWith(BerserkTitle);
		await Assert.That(fields.Keys).Contains("Publisher");
		await Assert.That(fields.Keys).Contains("Author");
		await Assert.That(fields["Author"]).Contains("Kentarou Miura");
	}

	[Test]
	public async Task TitleHonorsRussianPreference()
	{
		var media = new MangaSearchMedia
		{
			Id = 2UL,
			Name = BerserkTitle,
			RussianName = "Берсерк",
			Kind = "manga",
			Status = "ongoing",
			Url = MangaUrl,
		};

		var embed = SearchEmbedBuilder.Build(media, ListEntryType.Manga, ShikiUserFeatures.Russian, useRussian: true, RequesterDisplayName, AvatarUrl);

		await Assert.That(embed.Title).StartsWith("Берсерк");
	}
}
