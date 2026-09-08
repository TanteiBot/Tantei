// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests;

public sealed class ShikiFavouriteEmbedsTests
{
	private const uint AnimeId = 5114;

	private const uint CharacterId = 62;

	private const uint PersonId = 1870;

	private const uint BestKnownWorkId = 1;

	private const uint Episodes = 64;

	private const float AnimeScore = 9.1f;

	private const float BestKnownWorkScore = 8.5f;

	private const string BestKnownWorkName = "Best Known Show";

	private const string BestKnownWorkUrl = "https://shikimori.one/animes/1";

	private const string PersonTitle = "Fav Person";

	[Test]
	public async Task ADefaultFlagsMediaFavouriteRendersTheWholeIdentityBlock()
	{
		var embed = Build(MediaFavourite(), ShikiUserFeatures.Default);

		await Assert.That(embed.Title).IsEqualTo("Canonical Anime (Tv) [Released]");
		await Assert.That(FieldValue(embed, "Total")).IsEqualTo("64 ep.");
		await Assert.That(FieldValue(embed, "Score")).IsEqualTo("9.1");
		await Assert.That(embed.Thumbnail?.Url).IsEqualTo("https://shikimori.one/anime-original.jpg");
	}

	[Test]
	public async Task AMediaFavouriteFallsBackToTheEntryNameWhenEnrichmentIsMissing()
	{
		var embed = Build(new() { FavouriteEntry = AnimeEntry(), }, ShikiUserFeatures.Default);

		await Assert.That(embed.Title).IsEqualTo("Stored Anime [Animes]");
		await Assert.That(FieldNames(embed)).IsEmpty();
	}

	[Test]
	public async Task ACharacterRendersItsDescriptionOnlyUnderTheDescriptionFlag()
	{
		var withFlag = Build(CharacterFavourite(), ShikiUserFeatures.Default | ShikiUserFeatures.Description);
		var withoutFlag = Build(CharacterFavourite(), ShikiUserFeatures.Default);

		await Assert.That(FieldValue(withFlag, "Description")).IsEqualTo("A character bio.");
		await Assert.That(FieldNames(withoutFlag)).DoesNotContain("Description");
	}

	[Test]
	public async Task ACharacterBestKnownWorkIsLabelledFrom()
	{
		var embed = Build(CharacterFavourite(), ShikiUserFeatures.Default);

		await Assert.That(embed.Title).IsEqualTo("Fav Character [Character]");
		await Assert.That(FieldValue(embed, "From")).IsEqualTo($"[{BestKnownWorkName}]({BestKnownWorkUrl})");
		await Assert.That(FieldNames(embed)).DoesNotContain("Most popular work");
	}

	[Test]
	public async Task APersonRendersNoDescriptionEvenWithTheDescriptionFlagOn()
	{
		var embed = Build(PersonFavourite(), ShikiUserFeatures.Default | ShikiUserFeatures.Description);

		await Assert.That(embed.Title).IsEqualTo($"{PersonTitle} [Seyu]");
		await Assert.That(FieldValue(embed, "From")).IsEqualTo($"[{BestKnownWorkName}]({BestKnownWorkUrl})");
		await Assert.That(FieldNames(embed)).DoesNotContain("Description");
	}

	[Test]
	public async Task APersonSubKindPicksTheLabelSuffix()
	{
		var mangaka = Build(PersonFavourite(isMangaka: true), ShikiUserFeatures.Default);
		var producer = Build(PersonFavourite(isSeyu: false, isProducer: true), ShikiUserFeatures.Default);

		await Assert.That(mangaka.Title).IsEqualTo($"{PersonTitle} [Mangaka]");
		await Assert.That(producer.Title).IsEqualTo($"{PersonTitle} [Producer]");
	}

	[Test]
	public async Task ASeyuPicksItsBestKnownWorkFromRolesAndOtherPeopleFromWorks()
	{
		var details = new PersonDetails
		{
			Works = [new() { Anime = RelatedMedia("Themesong Show", id: 2, score: 1f), },],
			Roles = [new() { Animes = [RelatedMedia("Voiced Show", id: 3, score: 7f),], },],
		};

		await Assert.That(details.BestKnownWork(isSeyu: true)?.Name).IsEqualTo("Voiced Show");
		await Assert.That(details.BestKnownWork(isSeyu: false)?.Name).IsEqualTo("Themesong Show");
	}

	[Test]
	public async Task OnlyASeyuThatIsNotAlsoAMangakaPrefersRolesOverWorks()
	{
		await Assert.That(new FavouritePerson { IsSeyu = true, }.PrefersRolesOverWorks()).IsTrue();
		await Assert.That(new FavouritePerson { IsSeyu = true, IsMangaka = true, }.PrefersRolesOverWorks()).IsFalse();
		await Assert.That(new FavouritePerson { IsProducer = true, }.PrefersRolesOverWorks()).IsFalse();
	}

	[Test]
	public async Task ACharacterBestKnownWorkIsTheHighestScoredOfItsAnimeAndManga()
	{
		var details = new CharacterDetails
		{
			Animes = [RelatedMedia("Lesser Show", id: 4, score: 6f),],
			Mangas = [RelatedMedia("Better Manga", id: 5, score: 9f),],
		};

		await Assert.That(details.BestKnownWork()?.Name).IsEqualTo("Better Manga");
	}

	[Test]
	public async Task ARemovalRendersTheSameFieldsAsItsMatchingAddition()
	{
		var features = ShikiUserFeatures.Default | ShikiUserFeatures.Description;

		await AssertRemovalMatchesAdditionAsync(MediaFavourite(), features);
		await AssertRemovalMatchesAdditionAsync(CharacterFavourite(), features);
		await AssertRemovalMatchesAdditionAsync(PersonFavourite(), features);
	}

	private static async Task AssertRemovalMatchesAdditionAsync(EnrichedFavourite favourite, ShikiUserFeatures features)
	{
		var added = Build(favourite, features);
		var removed = Build(favourite, features, added: false);

		await Assert.That(removed.Title).IsEqualTo(added.Title);
		await Assert.That(removed.Url).IsEqualTo(added.Url);
		await Assert.That(removed.Thumbnail?.Url).IsEqualTo(added.Thumbnail?.Url);
		await Assert.That(removed.Fields.Select(static field => (field.Name, field.Value)))
					.IsEquivalentTo(added.Fields.Select(static field => (field.Name, field.Value)));
	}

	private static DiscordEmbedBuilder Build(EnrichedFavourite favourite, ShikiUserFeatures features, bool added = true) =>
		favourite.ToDiscordEmbed(CreateUser(), added, CreateDbUser(features));

	private static IEnumerable<string> FieldNames(DiscordEmbedBuilder embed) => embed.Fields.Select(static field => field.Name);

	private static string FieldValue(DiscordEmbedBuilder embed, string name) =>
		embed.Fields.Single(field => string.Equals(field.Name, name, StringComparison.Ordinal)).Value;

	private static RelatedMedia RelatedMedia(string name, uint id, float score) => new()
	{
		Id = id,
		Name = name,
		Score = score,
		Url = $"https://shikimori.one/animes/{id}",
	};

	private static EnrichedFavourite MediaFavourite() => new()
	{
		FavouriteEntry = AnimeEntry(),
		Media = new AnimeMedia
		{
			Id = AnimeId,
			Name = "Canonical Anime",
			Kind = "tv",
			Status = "released",
			Score = AnimeScore,
			Episodes = Episodes,
			Url = $"https://shikimori.one/animes/{AnimeId}",
			Poster = new() { OriginalUrl = "https://shikimori.one/anime-original.jpg", },
		},
	};

	private static EnrichedFavourite CharacterFavourite() => new()
	{
		FavouriteEntry = new() { Id = CharacterId, Name = "Stored Character", GenericType = "characters", },
		Character = new()
		{
			Id = CharacterId,
			Name = "Fav Character",
			Description = "A character bio.",
			Url = $"https://shikimori.one/characters/{CharacterId}",
			Poster = new() { OriginalUrl = "https://shikimori.one/character-original.jpg", },
		},
		BestKnownWork = RelatedMedia(BestKnownWorkName, BestKnownWorkId, BestKnownWorkScore),
	};

	private static EnrichedFavourite PersonFavourite(bool isSeyu = true, bool isMangaka = false, bool isProducer = false) => new()
	{
		FavouriteEntry = new() { Id = PersonId, Name = "Stored Person", GenericType = "people", },
		Person = new()
		{
			Id = PersonId,
			Name = PersonTitle,
			IsSeyu = isSeyu,
			IsMangaka = isMangaka,
			IsProducer = isProducer,
			Url = $"https://shikimori.one/people/{PersonId}",
			Poster = new() { OriginalUrl = "https://shikimori.one/person-original.jpg", },
		},
		BestKnownWork = RelatedMedia(BestKnownWorkName, BestKnownWorkId, BestKnownWorkScore),
	};

	private static FavouriteEntry AnimeEntry() => new()
	{
		Id = AnimeId,
		Name = "Stored Anime",
		GenericType = "animes",
		SpecificType = "animes",
	};

	private static UserInfo CreateUser() => new()
	{
		Id = 1,
		Nickname = "test-user",
	};

	private static ShikiUser CreateDbUser(ShikiUserFeatures features) => new()
	{
		Id = 1,
		DiscordUserId = 1,
		DiscordUser = new() { DiscordUserId = 1, BotUser = new(), Guilds = [], },
		Features = features,
		Colors = [],
		Favourites = [],
		Achievements = [],
	};
}
