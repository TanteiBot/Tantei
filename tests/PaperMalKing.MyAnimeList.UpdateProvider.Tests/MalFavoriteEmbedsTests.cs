// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests;

public sealed class MalFavoriteEmbedsTests
{
	private const double AnimeScore = 8.75;

	private const uint AnimeEpisodes = 24;

	private const uint AnimeId = 101;

	private const uint AnimeListUserCount = 1000;

	private const ushort AnimeStartYear = 2002;

	private const uint CharacterId = 303;

	private const uint CompanyId = 505;

	private const uint PersonId = 404;

	[Test]
	public async Task DefaultFlagsMediaFavoriteCostsOneCallAndRendersTheIdentityBlock()
	{
		var client = new FakeMyAnimeListFavoriteClient { AnimeResult = AnimeResult(), };

		var embed = await BuildSingleAsync(client, AnimeFavorite(), MalUserFeatures.Default);

		await Assert.That(client.TotalCallCount).IsEqualTo(1);
		await Assert.That(client.AnimeByIdCalls).IsEquivalentTo([AnimeId,]);
		await Assert.That(embed.Title).IsEqualTo("Canonical Anime (TV) [2002]");
		await Assert.That(FieldValue(embed, "Status")).IsEqualTo("Finished airing");
		await Assert.That(FieldValue(embed, "Score")).IsEqualTo("8.75");
		await Assert.That(FieldValue(embed, "Total")).IsEqualTo("24 ep.");
		await Assert.That(embed.Thumbnail?.Url).IsEqualTo("https://cdn.myanimelist.net/anime/large.jpg");
	}

	[Test]
	public async Task AFailedOfficialFetchDropsOnlyItsFieldsAndLogsAWarning()
	{
		var client = new FakeMyAnimeListFavoriteClient { OfficialException = new HttpRequestException("boom"), };
		var logger = new RecordingLogger<BaseUpdateProvider>();

		var embed = await BuildSingleAsync(client, AnimeFavorite(), MalUserFeatures.Default, logger);

		await Assert.That(embed.Title).IsEqualTo("Fav Anime (TV) [2002]");
		await Assert.That(FieldNames(embed)).IsEmpty();
		await Assert.That(embed.Thumbnail?.Url).IsEqualTo("https://cdn.myanimelist.net/anime/stored.jpg");
		await Assert.That(logger.Entries.Select(static entry => entry.EventId.Name)).Contains("FailedToEnrichFavorite");
	}

	[Test]
	public async Task AFailedOfficialFetchStillLeavesTheTenraiSourcedFields()
	{
		var client = new FakeMyAnimeListFavoriteClient
		{
			OfficialException = new HttpRequestException("boom"),
			AnimeSeiyuResult = [new() { Name = "A Seiyu", Url = "https://myanimelist.net/people/7", },],
		};

		var embed = await BuildSingleAsync(client, AnimeFavorite(), MalUserFeatures.Default | MalUserFeatures.Seiyu);

		await Assert.That(client.AnimeSeiyuCalls).IsEquivalentTo([(long)AnimeId,]);
		await Assert.That(FieldValue(embed, "Seiyu")).IsEqualTo("[A Seiyu](https://myanimelist.net/people/7)");
	}

	[Test]
	public async Task ACharacterBestKnownWorkIsAMaskedLinkLabelledFrom()
	{
		var client = new FakeMyAnimeListFavoriteClient
		{
			CharacterInfoResult = new()
			{
				Description = "A bio.",
				BestKnownWork = new() { Title = "Main Show", Url = "https://myanimelist.net/anime/1", },
			},
		};

		var embed = await BuildSingleAsync(client, CharacterFavorite(), MalUserFeatures.Default | MalUserFeatures.Synopsis);

		await Assert.That(client.CharacterInfoCalls).IsEquivalentTo([((long)CharacterId, true),]);
		await Assert.That(embed.Title).IsEqualTo("Fav Character [Character]");
		await Assert.That(FieldValue(embed, "From")).IsEqualTo("[Main Show](https://myanimelist.net/anime/1)");
		await Assert.That(FieldValue(embed, "Description")).IsEqualTo("A bio.");
	}

	[Test]
	public async Task ACharacterWithoutEnrichmentKeepsTheStoredFromTitleAndDropsTheDescription()
	{
		var client = new FakeMyAnimeListFavoriteClient();

		var embed = await BuildSingleAsync(client, CharacterFavorite(), MalUserFeatures.Default);

		await Assert.That(client.CharacterInfoCalls).IsEquivalentTo([((long)CharacterId, false),]);
		await Assert.That(FieldValue(embed, "From")).IsEqualTo("Stored Show");
		await Assert.That(FieldNames(embed)).DoesNotContain("Description");
	}

	[Test]
	public async Task APersonFavoriteIsLabelledPersonAndAStudioFavoriteIsKnownFor()
	{
		var client = new FakeMyAnimeListFavoriteClient
		{
			PersonInfoResult = new() { BestKnownWork = new() { Title = "Voiced Show", Url = "https://myanimelist.net/anime/2", }, },
			StudioInfoResult = new() { BestKnownWork = new() { Title = "Made Show", Url = "https://myanimelist.net/anime/3", }, },
		};

		var person = await BuildSingleAsync(client, PersonFavorite(), MalUserFeatures.Default);
		var studio = await BuildSingleAsync(client, CompanyFavorite(), MalUserFeatures.Default);

		await Assert.That(person.Title).IsEqualTo("Fav Person [Person]");
		await Assert.That(FieldValue(person, "From")).IsEqualTo("[Voiced Show](https://myanimelist.net/anime/2)");
		await Assert.That(studio.Title).IsEqualTo("Fav Studio [Studio]");
		await Assert.That(FieldValue(studio, "Known for")).IsEqualTo("[Made Show](https://myanimelist.net/anime/3)");
	}

	[Test]
	public async Task AFailedTenraiFetchDropsOnlyItsFieldsAndLogsAWarning()
	{
		var client = new FakeMyAnimeListFavoriteClient { TenraiException = new HttpRequestException("boom"), };
		var logger = new RecordingLogger<BaseUpdateProvider>();

		var embed = await BuildSingleAsync(client, CharacterFavorite(), MalUserFeatures.Default | MalUserFeatures.Synopsis, logger);

		await Assert.That(embed.Title).IsEqualTo("Fav Character [Character]");
		await Assert.That(FieldValue(embed, "From")).IsEqualTo("Stored Show");
		await Assert.That(FieldNames(embed)).DoesNotContain("Description");
		await Assert.That(logger.Entries.Select(static entry => entry.EventId.Name)).Contains("FailedToEnrichFavorite");
	}

	[Test]
	public async Task AFailedTenraiFetchLeavesTheOfficiallySourcedMediaFields()
	{
		var client = new FakeMyAnimeListFavoriteClient
		{
			AnimeResult = AnimeResult(),
			TenraiException = new HttpRequestException("boom"),
		};

		var embed = await BuildSingleAsync(client, AnimeFavorite(), MalUserFeatures.Default | MalUserFeatures.Seiyu | MalUserFeatures.Themes);

		await Assert.That(embed.Title).IsEqualTo("Canonical Anime (TV) [2002]");
		await Assert.That(FieldValue(embed, "Score")).IsEqualTo("8.75");
		await Assert.That(FieldNames(embed)).DoesNotContain("Seiyu");
		await Assert.That(FieldNames(embed)).DoesNotContain("Themes");
	}

	[Test]
	public async Task ARemovalRendersTheSameFieldsAsItsMatchingAddition()
	{
		var client = new FakeMyAnimeListFavoriteClient { AnimeResult = AnimeResult(), };
		var dbUser = CreateMalUser(MalUserFeatures.Default);

		var embeds = await MalFavoriteEmbeds.BuildAsync(
			[
				new() { Favorite = AnimeFavorite(), Added = true, }, new() { Favorite = AnimeFavorite(), Added = false, },
			],
			CreateUser(), dbUser, client, new RecordingLogger<BaseUpdateProvider>(), CancellationToken.None);

		var added = embeds.Single(static embed => string.Equals(embed.Description, "Added favorite", StringComparison.Ordinal));
		var removed = embeds.Single(static embed => string.Equals(embed.Description, "Removed favorite", StringComparison.Ordinal));
		await Assert.That(removed.Title).IsEqualTo(added.Title);
		await Assert.That(removed.Fields.Select(static field => (field.Name, field.Value)))
					.IsEquivalentTo(added.Fields.Select(static field => (field.Name, field.Value)));
	}

	private static async Task<DiscordEmbedBuilder> BuildSingleAsync(
		FakeMyAnimeListFavoriteClient client,
		BaseMalFavorite favorite,
		MalUserFeatures features,
		RecordingLogger<BaseUpdateProvider>? logger = null)
	{
		var embeds = await MalFavoriteEmbeds.BuildAsync([new() { Favorite = favorite, Added = true, },], CreateUser(), CreateMalUser(features),
			client, logger ?? new RecordingLogger<BaseUpdateProvider>(), CancellationToken.None);
		return embeds.Single();
	}

	private static IEnumerable<string> FieldNames(DiscordEmbedBuilder embed) => embed.Fields.Select(static field => field.Name);

	private static string FieldValue(DiscordEmbedBuilder embed, string name) =>
		embed.Fields.Single(field => string.Equals(field.Name, name, StringComparison.Ordinal)).Value;

	private static AnimeSearchResult AnimeResult() => new()
	{
		Id = AnimeId,
		PrimaryTitle = "Canonical Anime",
		MediaType = AnimeMediaType.TV,
		Status = AnimeAiringStatus.FinishedAiring,
		Episodes = AnimeEpisodes,
		Mean = AnimeScore,
		ListUserCount = AnimeListUserCount,
		Picture = new() { Medium = "https://cdn.myanimelist.net/anime/medium.jpg", Large = "https://cdn.myanimelist.net/anime/large.jpg", },
	};

	private static MalFavoriteAnime AnimeFavorite() => new()
	{
		Id = AnimeId,
		Name = "Fav Anime",
		Type = "TV",
		StartYear = AnimeStartYear,
		ImageUrl = "https://cdn.myanimelist.net/anime/stored.jpg",
		NameUrl = "https://myanimelist.net/anime/101",
		User = CreateMalUser(MalUserFeatures.Default),
	};

	private static MalFavoriteCharacter CharacterFavorite() => new()
	{
		Id = CharacterId,
		Name = "Fav Character",
		FromTitleName = "Stored Show",
		ImageUrl = "https://cdn.myanimelist.net/character.jpg",
		NameUrl = "https://myanimelist.net/character/303",
		User = CreateMalUser(MalUserFeatures.Default),
	};

	private static MalFavoritePerson PersonFavorite() => new()
	{
		Id = PersonId,
		Name = "Fav Person",
		ImageUrl = "https://cdn.myanimelist.net/person.jpg",
		NameUrl = "https://myanimelist.net/people/404",
		User = CreateMalUser(MalUserFeatures.Default),
	};

	private static MalFavoriteCompany CompanyFavorite() => new()
	{
		Id = CompanyId,
		Name = "Fav Studio",
		ImageUrl = "https://cdn.myanimelist.net/company.jpg",
		NameUrl = "https://myanimelist.net/anime/producer/505",
		User = CreateMalUser(MalUserFeatures.Default),
	};

	private static User CreateUser() => new()
	{
		Id = 1,
		Username = "Test User",
		Favorites = UserFavorites.Empty,
	};

	private static MalUser CreateMalUser(MalUserFeatures features) => new()
	{
		DiscordUserId = 1,
		DiscordUser = new() { DiscordUserId = 1, BotUser = new(), Guilds = [], },
		UserId = 1,
		Username = "test-user",
		Features = features,
		Colors = [],
	};
}
