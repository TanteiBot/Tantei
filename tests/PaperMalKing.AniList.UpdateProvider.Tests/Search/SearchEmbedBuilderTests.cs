// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.AniList.UpdateProvider.Search;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Database.Models.AniList;
using TUnit.Assertions.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Tests.Search;

public sealed class SearchEmbedBuilderTests
{
	private const string RequesterDisplayName = "nodan";
	private const string AvatarUrl = "https://cdn.discordapp.com/avatars/1/abc.png";
	private const string Banner = "https://img.anili.st/media/1";
	private const string MediaUrl = "https://anilist.co/anime/1";
	private const string Description = "A short description.";
	private const int AniListBlueValue = 0x3DB4F2;
	private const ushort EpisodeCount = 12;
	private const uint Popularity = 1_400_000;
	private const ushort AverageScore = 85;
	private const ushort SeasonYear = 2004;
	private const byte PrimaryTagRank = 80;
	private const byte SpoilerTagRank = 90;
	private const string ActionTag = "Action";
	private const string MonsterTitle = "Monster";

	private const AniListUserFeatures RichFeatures =
		AniListUserFeatures.MediaFormat | AniListUserFeatures.MediaStatus | AniListUserFeatures.Tags | AniListUserFeatures.MediaDescription;

	[Test]
	public async Task BannerDrivenEmbedKeepsTheChromeAndLeanFieldSet()
	{
		var media = Media(popularity: Popularity, episodes: EpisodeCount, description: Description, tags: [Tag(ActionTag, PrimaryTagRank)]);

		var embed = SearchEmbedBuilder.Build(media, RichFeatures, TitleLanguage.Romaji, RequesterDisplayName, AvatarUrl);

		await Assert.That(embed.Title).StartsWith(MonsterTitle);
		await Assert.That(embed.Url).IsEqualTo(MediaUrl);
		await Assert.That(embed.ImageUrl).IsEqualTo(Banner);
		await Assert.That(embed.Thumbnail).IsNull();
		await Assert.That(embed.Color.Value).IsEqualTo(AniListBlueValue);
		await Assert.That(embed.Author.Name).IsEqualTo("Requested by nodan");
		await Assert.That(embed.Author.IconUrl).IsEqualTo(AvatarUrl);
		await Assert.That(embed.Footer.Text).IsEqualTo("AniList");
		await Assert.That(embed.Fields.Select(static field => (field.Name, field.Value, field.Inline))).IsEquivalentTo(
			[
				("Total", "12 ep.", true),
				("Popularity", "1.4M", true),
				("Tags", ActionTag, true),
				("Description", Description, true),
			],
			CollectionOrdering.Matching);
	}

	[Test]
	public async Task PopularityFieldIsLabelledPopularityAndHumanized()
	{
		var media = Media(popularity: Popularity);

		var embed = SearchEmbedBuilder.Build(media, RichFeatures, TitleLanguage.Romaji, RequesterDisplayName, AvatarUrl);

		var popularity = embed.Fields.Single(static field => string.Equals(field.Name, "Popularity", StringComparison.Ordinal));
		await Assert.That(popularity.Value).IsEqualTo("1.4M");
		await Assert.That(embed.Fields.Select(static field => field.Name)).DoesNotContain("Members");
	}

	[Test]
	public async Task ScoreSeasonAndGenresAreDropped()
	{
		var media = Media(averageScore: AverageScore, seasonYear: SeasonYear);

		var embed = SearchEmbedBuilder.Build(media, RichFeatures, TitleLanguage.Romaji, RequesterDisplayName, AvatarUrl);

		var fieldNames = embed.Fields.Select(static field => field.Name).ToArray();
		await Assert.That(fieldNames).DoesNotContain("Score");
		await Assert.That(fieldNames).DoesNotContain("Season");
		await Assert.That(fieldNames).DoesNotContain("Genres");
	}

	[Test]
	public async Task TagsAndDescriptionAreFeatureGated()
	{
		var media = Media(description: Description, tags: [Tag(ActionTag, PrimaryTagRank)]);

		var embed = SearchEmbedBuilder.Build(
			media,
			AniListUserFeatures.MediaFormat | AniListUserFeatures.MediaStatus,
			TitleLanguage.Romaji,
			RequesterDisplayName,
			AvatarUrl);

		var fieldNames = embed.Fields.Select(static field => field.Name).ToArray();
		await Assert.That(fieldNames).DoesNotContain("Tags");
		await Assert.That(fieldNames).DoesNotContain("Description");
	}

	[Test]
	public async Task AdultResultStillUsesTheBannerNotAThumbnail()
	{
		var media = Media(isAdult: true);

		var embed = SearchEmbedBuilder.Build(media, RichFeatures, TitleLanguage.Romaji, RequesterDisplayName, AvatarUrl);

		await Assert.That(embed.Thumbnail).IsNull();
		await Assert.That(embed.ImageUrl).IsEqualTo(Banner);
	}

	[Test]
	public async Task EnrichWithMediaInfoStaysByteIdenticalAfterTextInfoExtraction()
	{
		var media = new Media
		{
			Title = new() { Romaji = MonsterTitle },
			Url = MediaUrl,
			Type = ListType.Anime,
			Description = Description,
			Tags = [Tag(ActionTag, PrimaryTagRank), Tag("Spoiler", SpoilerTagRank, isSpoiler: true)],
		};
		const AniListUserFeatures features = AniListUserFeatures.Tags | AniListUserFeatures.MediaDescription;

		var full = new DiscordEmbedBuilder().EnrichWithMediaInfo(media, user: null, features);
		var textOnly = new DiscordEmbedBuilder().EnrichWithTextInfo(media, features);

		await Assert.That(full.Fields.Select(static field => (field.Name, field.Value, field.Inline))).IsEquivalentTo(
			textOnly.Fields.Select(static field => (field.Name, field.Value, field.Inline)),
			CollectionOrdering.Matching);
	}

	private static MediaTag Tag(string name, byte rank, bool isSpoiler = false) => new()
	{
		Name = name,
		Rank = rank,
		IsSpoiler = isSpoiler,
	};

	private static SearchMedia Media(
		MediaFormat? format = MediaFormat.TV,
		MediaStatus status = MediaStatus.Finished,
		uint popularity = Popularity,
		ushort? episodes = EpisodeCount,
		ushort? averageScore = AverageScore,
		ushort? seasonYear = SeasonYear,
		bool isAdult = false,
		string? description = null,
		IReadOnlyList<MediaTag>? tags = null) => new()
		{
			Id = 1,
			Title = new() { Romaji = MonsterTitle },
			Url = MediaUrl,
			Type = ListType.Anime,
			Format = format,
			Status = status,
			Popularity = popularity,
			Episodes = episodes,
			AverageScore = averageScore,
			SeasonYear = seasonYear,
			IsAdult = isAdult,
			Description = description,
			Tags = tags ?? [],
		};
}
