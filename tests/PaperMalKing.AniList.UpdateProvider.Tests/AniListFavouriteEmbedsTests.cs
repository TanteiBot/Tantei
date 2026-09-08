// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Database.Models.AniList;

namespace PaperMalKing.AniList.UpdateProvider.Tests;

public sealed class AniListFavouriteEmbedsTests
{
	private const ushort AverageScore = 85;

	private const ushort Episodes = 24;

	private const uint CharacterId = 10;

	private const uint StaffId = 20;

	private const uint StudioId = 30;

	private const string PopularShow = "Popular Show";

	[Test]
	public async Task ADefaultFlagsMediaFavouriteRendersTheIdentityBlock()
	{
		var embed = Build(Media(PopularShow), AniListUserFeatures.Default);

		await Assert.That(embed.Title).IsEqualTo($"{PopularShow} (TV) [Finished]");
		await Assert.That(FieldValue(embed, "Total")).IsEqualTo("24 ep.");
		await Assert.That(FieldValue(embed, "Score")).IsEqualTo("85/100");
		await Assert.That(embed.Thumbnail?.Url).IsEqualTo("https://anilist.co/media.jpg");
	}

	[Test]
	public async Task ACharacterRendersItsDescriptionOnlyUnderTheDescriptionFlag()
	{
		var withFlag = Build(Character(), AniListUserFeatures.Default | AniListUserFeatures.Description);
		var withoutFlag = Build(Character(), AniListUserFeatures.Default);

		await Assert.That(FieldValue(withFlag, "Description")).IsEqualTo("A character bio.");
		await Assert.That(FieldNames(withoutFlag)).DoesNotContain("Description");
	}

	[Test]
	public async Task ACharacterBestKnownWorkIsLabelledFrom()
	{
		var embed = Build(Character(), AniListUserFeatures.Default);

		await Assert.That(embed.Title).IsEqualTo("Fav Character [Character]");
		await Assert.That(FieldValue(embed, "From")).IsEqualTo($"[{PopularShow}](https://anilist.co/anime/1)");
		await Assert.That(FieldNames(embed)).DoesNotContain("Most popular work");
		await Assert.That(FieldNames(embed)).DoesNotContain("Most popular title");
	}

	[Test]
	public async Task AVoiceActorsBestKnownWorkComesFromCharacterMediaAndOtherStaffsFromStaffMedia()
	{
		var voiceActor = Build(Staff("Voice Actor"), AniListUserFeatures.Default);
		var director = Build(Staff("Director"), AniListUserFeatures.Default);

		await Assert.That(voiceActor.Title).IsEqualTo("Fav Staff [Voice Actor]");
		await Assert.That(FieldValue(voiceActor, "From")).IsEqualTo("[Voiced Show](https://anilist.co/anime/2)");
		await Assert.That(director.Title).IsEqualTo("Fav Staff [Director]");
		await Assert.That(FieldValue(director, "From")).IsEqualTo("[Directed Show](https://anilist.co/anime/3)");
	}

	[Test]
	public async Task AStudioIsLabelledKnownForAndCarriesNoPictureAtAll()
	{
		var embed = Build(Studio(), AniListUserFeatures.Default);

		await Assert.That(embed.Title).IsEqualTo("Fav Studio [Studio]");
		await Assert.That(FieldValue(embed, "Known for")).IsEqualTo("[Made Show](https://anilist.co/anime/4)");
		await Assert.That(embed.Thumbnail?.Url).IsNullOrEmpty();
		await Assert.That(embed.ImageUrl).IsNullOrEmpty();
	}

	[Test]
	public async Task ARemovalRendersTheSameFieldsAsItsMatchingAddition()
	{
		var features = AniListUserFeatures.Default | AniListUserFeatures.Description;

		await AssertRemovalMatchesAdditionAsync(Build(Media(PopularShow), features), Build(Media(PopularShow), features, added: false));
		await AssertRemovalMatchesAdditionAsync(Build(Character(), features), Build(Character(), features, added: false));
		await AssertRemovalMatchesAdditionAsync(Build(Staff("Voice Actor"), features), Build(Staff("Voice Actor"), features, added: false));
		await AssertRemovalMatchesAdditionAsync(Build(Studio(), features), Build(Studio(), features, added: false));
	}

	private static async Task AssertRemovalMatchesAdditionAsync(DiscordEmbedBuilder added, DiscordEmbedBuilder removed)
	{
		await Assert.That(removed.Title).IsEqualTo(added.Title);
		await Assert.That(removed.Thumbnail?.Url).IsEqualTo(added.Thumbnail?.Url);
		await Assert.That(removed.Fields.Select(static field => (field.Name, field.Value)))
					.IsEquivalentTo(added.Fields.Select(static field => (field.Name, field.Value)));
	}

	private static DiscordEmbedBuilder Build(Media favourite, AniListUserFeatures features, bool added = true) =>
		FavouriteToDiscordEmbedBuilderConverter.Convert(favourite, CreateUser(), added, CreateDbUser(features));

	private static DiscordEmbedBuilder Build(Character favourite, AniListUserFeatures features, bool added = true) =>
		FavouriteToDiscordEmbedBuilderConverter.Convert(favourite, CreateUser(), added, CreateDbUser(features));

	private static DiscordEmbedBuilder Build(Staff favourite, AniListUserFeatures features, bool added = true) =>
		FavouriteToDiscordEmbedBuilderConverter.Convert(favourite, CreateUser(), added, CreateDbUser(features));

	private static DiscordEmbedBuilder Build(Studio favourite, AniListUserFeatures features, bool added = true) =>
		FavouriteToDiscordEmbedBuilderConverter.Convert(favourite, CreateUser(), added, CreateDbUser(features));

	private static IEnumerable<string> FieldNames(DiscordEmbedBuilder embed) => embed.Fields.Select(static field => field.Name);

	private static string FieldValue(DiscordEmbedBuilder embed, string name) =>
		embed.Fields.Single(field => string.Equals(field.Name, name, StringComparison.Ordinal)).Value;

	private static Media Media(string title, uint id = 1) => new()
	{
		Id = id,
		Title = new() { Romaji = title, Native = title, },
		Type = ListType.Anime,
		Url = $"https://anilist.co/anime/{id}",
		Format = MediaFormat.TV,
		Status = MediaStatus.Finished,
		Episodes = Episodes,
		AverageScore = AverageScore,
		Image = new() { ImageUrl = "https://anilist.co/media.jpg", },
	};

	private static Character Character() => new()
	{
		Id = CharacterId,
		Name = new() { Full = "Fav Character", Native = "Fav Character", },
		Url = "https://anilist.co/character/10",
		Image = new() { ImageUrl = "https://anilist.co/character.jpg", },
		Description = "A character bio.",
		Media = new() { Values = [Media(PopularShow)], },
	};

	private static Staff Staff(string occupation) => new()
	{
		Id = StaffId,
		Name = new() { Full = "Fav Staff", Native = "Fav Staff", },
		Url = "https://anilist.co/staff/20",
		Image = new() { ImageUrl = "https://anilist.co/staff.jpg", },
		Description = "A staff bio.",
		PrimaryOccupations = [occupation],
		CharacterMedia = new() { Nodes = [Media("Voiced Show", id: 2)], },
		StaffMedia = new() { Nodes = [Media("Directed Show", id: 3)], },
	};

	private static Studio Studio() => new()
	{
		Id = StudioId,
		Name = "Fav Studio",
		Url = "https://anilist.co/studio/30",
		Media = new() { Nodes = [Media("Made Show", id: 4)], },
	};

	private static User CreateUser() => new()
	{
		Id = 1,
		Name = "Test User",
		Url = "https://anilist.co/user/1",
		Options = new() { TitleLanguage = TitleLanguage.Romaji, },
	};

	private static AniListUser CreateDbUser(AniListUserFeatures features) => new()
	{
		Id = 1,
		DiscordUserId = 1,
		DiscordUser = new() { DiscordUserId = 1, BotUser = new(), Guilds = [], },
		Features = features,
		Colors = [],
		Favourites = [],
	};
}
