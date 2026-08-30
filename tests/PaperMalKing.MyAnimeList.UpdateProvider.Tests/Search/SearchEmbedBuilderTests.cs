// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using TUnit.Assertions.Enums;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class SearchEmbedBuilderTests
{
	private const int AuthorNameLimit = 256;
	private const int FieldValueLimit = 1024;
	private const int MalBlueValue = 0x26448F;
	private const int SynopsisLimit = 500;
	private const uint MangaChapterCount = 84U;
	private const uint MangaVolumeCount = 9U;
	private const string AvatarUrl = "https://cdn.discordapp.com/avatars/1/avatar.png";
	private const string DisplayName = "nodan";
	private const string MediumPosterUrl = "https://cdn.myanimelist.net/medium.jpg";

	[Test]
	public async Task CompleteAnimeEmbedHasTheFixedPublicShapeWithoutFeatureGating()
	{
		var result = Anime(
			id: 19U,
			title: "Monster",
			picture: new() { Large = "https://cdn.myanimelist.net/large.jpg", Medium = MediumPosterUrl, },
			mean: 8.88,
			episodes: 74U,
			listUserCount: 1_360_412U,
			season: new() { Season = AnimeSeason.Spring, Year = 2004U, },
			genres: ["Award Winning", "Drama", "Mystery", "Suspense",],
			synopsis: "Inspector Kenzou Tenma searches for the truth.");

		var embed = SearchEmbedBuilder.Build(result, DisplayName, AvatarUrl);

		await Assert.That(embed.Title).IsEqualTo("Monster");
		await Assert.That(embed.Url).IsEqualTo("https://myanimelist.net/anime/19");
		await Assert.That(embed.Description).IsEqualTo("Inspector Kenzou Tenma searches for the truth.");
		await Assert.That(embed.Thumbnail.Url).IsEqualTo("https://cdn.myanimelist.net/large.jpg");
		await Assert.That(embed.Color.Value).IsEqualTo(MalBlueValue);
		await Assert.That(embed.Author.Name).IsEqualTo("Requested by nodan");
		await Assert.That(embed.Author.IconUrl).IsEqualTo(AvatarUrl);
		await Assert.That(embed.Author.Url).IsNull();
		await Assert.That(embed.Footer.Text).IsEqualTo("MyAnimeList");
		await Assert.That(embed.Footer.IconUrl).IsEqualTo("https://cdn.myanimelist.net/images/MalAppIcon_180px.png");
		await Assert.That(embed.Timestamp).IsNull();
		await Assert.That(embed.Fields.Select(static field => (field.Name, field.Value, field.Inline))).IsEquivalentTo(
			[
				("Type", "TV", true),
				("Status", "Finished airing", true),
				("Score", "8.88", true),
				("Total", "74 ep.", true),
				("Season", "Spring 2004", true),
				("Members", "1,360,412", true),
				("Genres", "Award Winning, Drama, Mystery, Suspense", false),
			],
			CollectionOrdering.Matching);
	}

	[Test]
	public async Task CompleteMangaEmbedUsesMangaLinkAndTotals()
	{
		var result = Manga(
			id: 2U,
			title: "Berserk",
			picture: new() { Medium = MediumPosterUrl, },
			mean: 9.47,
			chapters: 84U,
			volumes: 9U,
			listUserCount: 735_311U,
			genres: ["Action", "Adventure",],
			synopsis: "Guts pursues his own dream.");

		var embed = SearchEmbedBuilder.Build(result, DisplayName, AvatarUrl);

		await Assert.That(embed.Url).IsEqualTo("https://myanimelist.net/manga/2");
		await Assert.That(embed.Thumbnail.Url).IsEqualTo(MediumPosterUrl);
		await Assert.That(embed.Fields.Select(static field => (field.Name, field.Value, field.Inline))).IsEquivalentTo(
			[
				("Type", "Manga", true),
				("Status", "Currently publishing", true),
				("Score", "9.47", true),
				("Total", "84 ch, 9 v.", true),
				("Members", "735,311", true),
				("Genres", "Action, Adventure", false),
			],
			CollectionOrdering.Matching);
	}

	[Test]
	[Arguments(MangaChapterCount, 0U, "84 ch")]
	[Arguments(0U, MangaVolumeCount, "9 v.")]
	[Arguments(0U, 0U, null)]
	public async Task MangaTotalOmitsAbsentCounts(uint chapters, uint volumes, string? expected)
	{
		var result = Manga(
			id: 2U,
			title: "Berserk",
			picture: null,
			mean: null,
			chapters,
			volumes,
			listUserCount: 1U,
			genres: [],
			synopsis: null);

		var embed = SearchEmbedBuilder.Build(result, DisplayName, AvatarUrl);
		var total = embed.Fields.SingleOrDefault(static field => string.Equals(field.Name, "Total", StringComparison.Ordinal))?.Value;

		await Assert.That(total).IsEqualTo(expected);
	}

	[Test]
	public async Task MediumPosterIsUsedWhenLargePosterIsUnusable()
	{
		var picture = new Picture { Large = " ", Medium = MediumPosterUrl, };

		var embed = SearchEmbedBuilder.Build(Anime(picture: picture), DisplayName, AvatarUrl);

		await Assert.That(embed.Thumbnail.Url).IsEqualTo(MediumPosterUrl);
	}

	[Test]
	public async Task OptionalValuesAreOmitted()
	{
		var result = Anime(
			id: 1U,
			title: "Unscored",
			picture: null,
			mean: null,
			episodes: 0U,
			listUserCount: 0U,
			season: null,
			genres: [],
			synopsis: null,
			mediaType: AnimeMediaType.Unknown,
			status: AnimeAiringStatus.Unknown);

		var embed = SearchEmbedBuilder.Build(result, DisplayName, avatarUrl: null);

		await Assert.That(embed.Description).IsNull();
		await Assert.That(embed.Thumbnail).IsNull();
		await Assert.That(embed.Author.IconUrl).IsNull();
		await Assert.That(embed.Fields.Select(static field => field.Name)).IsEquivalentTo(["Members",]);
		await Assert.That(embed.Fields[0].Value).IsEqualTo("0");
	}

	[Test]
	[Arguments("A complete synopsis. (Source: MyAnimeList)")]
	[Arguments("A complete synopsis. [Written by MAL Rewrite]")]
	public async Task SourceCreditTailIsRemoved(string synopsis)
	{
		var embed = SearchEmbedBuilder.Build(Anime(synopsis: synopsis), DisplayName, AvatarUrl);

		await Assert.That(embed.Description).IsEqualTo("A complete synopsis.");
	}

	[Test]
	public async Task SynopsisIsTruncatedToFiveHundredCharactersWithEllipsis()
	{
		var embed = SearchEmbedBuilder.Build(Anime(synopsis: new('a', 600)), DisplayName, AvatarUrl);

		await Assert.That(embed.Description).IsNotNull();
		await Assert.That(embed.Description.Length).IsEqualTo(SynopsisLimit);
		await Assert.That(embed.Description.EndsWith('…')).IsTrue();
	}

	[Test]
	public async Task PathologicalTitleMovesTheCompleteLinkToDescriptionAndDropsSynopsis()
	{
		var title = new string('x', 257);

		var embed = SearchEmbedBuilder.Build(Anime(title: title, synopsis: "This synopsis is replaced."), DisplayName, AvatarUrl);

		await Assert.That(embed.Title).IsNull();
		await Assert.That(embed.Url).IsNull();
		await Assert.That(embed.Description).IsEqualTo($"[{title}](https://myanimelist.net/anime/1)");
	}

	[Test]
	public async Task GenresAreCappedAtSeven()
	{
		var genres = Enumerable.Range(1, 8).Select(static number => string.Create(CultureInfo.InvariantCulture, $"Genre {number}")).ToArray();

		var embed = SearchEmbedBuilder.Build(Anime(genres: genres), DisplayName, AvatarUrl);
		var genresField = embed.Fields[^1];

		await Assert.That(genresField.Name).IsEqualTo("Genres");
		await Assert.That(genresField.Value).IsEqualTo("Genre 1, Genre 2, Genre 3, Genre 4, Genre 5, Genre 6, Genre 7");
		await Assert.That(genresField.Inline).IsFalse();
	}

	[Test]
	public async Task DiscordFieldAndAuthorLimitsAreEnforced()
	{
		var genres = Enumerable.Range(1, 7).Select(static number => string.Create(CultureInfo.InvariantCulture, $"Genre {number}{new string('x', 200)}")).ToArray();

		var embed = SearchEmbedBuilder.Build(Anime(genres: genres), new string('n', 300), AvatarUrl);

		await Assert.That(embed.Fields[^1].Value.Length).IsLessThanOrEqualTo(FieldValueLimit);
		await Assert.That(embed.Author.Name.Length).IsLessThanOrEqualTo(AuthorNameLimit);
	}

	private static AnimeSearchResult Anime(
		uint id = 1U,
		string title = "Monster",
		Picture? picture = null,
		double? mean = null,
		uint episodes = 0U,
		uint listUserCount = 1U,
		AnimeStartSeason? season = null,
		IReadOnlyList<string>? genres = null,
		string? synopsis = null,
		AnimeMediaType mediaType = AnimeMediaType.TV,
		AnimeAiringStatus status = AnimeAiringStatus.FinishedAiring) => new()
		{
			Id = id,
			PrimaryTitle = title,
			Picture = picture,
			MediaType = mediaType,
			Status = status,
			Mean = mean,
			Episodes = episodes,
			ListUserCount = listUserCount,
			Genres = ToGenres(genres),
			Synopsis = synopsis,
			StartSeason = season,
		};

	private static MangaSearchResult Manga(
		uint id,
		string title,
		Picture? picture,
		double? mean,
		uint chapters,
		uint volumes,
		uint listUserCount,
		IReadOnlyList<string> genres,
		string? synopsis) => new()
		{
			Id = id,
			PrimaryTitle = title,
			Picture = picture,
			MediaType = MangaMediaType.Manga,
			Status = MangaPublishingStatus.CurrentlyPublishing,
			Mean = mean,
			Chapters = chapters,
			Volumes = volumes,
			ListUserCount = listUserCount,
			Genres = ToGenres(genres),
			Synopsis = synopsis,
		};

	private static Genre[] ToGenres(IReadOnlyList<string>? genres) =>
		genres?.Select(static name => new Genre { Name = name, }).ToArray() ?? [];
}
