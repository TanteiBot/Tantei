// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests;

public sealed class ShikiFavouriteEnrichmentTests
{
	private const uint AnimeId = 5114;

	private const uint CharacterId = 62;

	private const uint PersonId = 1870;

	private const int MediaFavouriteCount = 5;

	private const int MoreIdsThanOneBatchHolds = 51;

	private const uint AddedAnimeId = 1;

	private const uint RemovedAnimeId = 2;

	private const string RolesWorkName = "Best Known Role";

	private const string WorksWorkName = "Best Known Work";

	private const string PersonName = "Fav Person";

	[Test]
	public async Task ADefaultFlagsMediaFavouriteCostsNoEnrichmentCall()
	{
		var client = new FakeShikiFavouriteClient { FavouritesInfo = new() { Animes = [AnimeMedia(AnimeId),], }, };

		var favourite = await EnrichOneAsync(client, AnimeEntry(AnimeId));

		await Assert.That(client.FavouritesInfoCalls).Count().IsEqualTo(1);
		await Assert.That(client.EnrichmentCallCount).IsEqualTo(0);
		await Assert.That(favourite.Media?.Name).IsEqualTo($"Canonical Anime {AnimeId}");
	}

	[Test]
	public async Task EveryChangedMediaFavouriteOfATickRidesOneCallForFavouriteInfo()
	{
		var entries = Enumerable.Range(1, MediaFavouriteCount).Select(static i => AnimeEntry((uint)i)).ToArray();
		var client = new FakeShikiFavouriteClient { FavouritesInfo = new() { Animes = [.. entries.Select(static entry => AnimeMedia(entry.Id))], }, };

		var (added, _) = await EnrichAsync(client, entries, []);

		await Assert.That(client.FavouritesInfoCalls).Count().IsEqualTo(1);
		await Assert.That(client.FavouritesInfoCalls[0].AnimeIds).IsEquivalentTo(entries.Select(static entry => entry.Id));
		await Assert.That(client.EnrichmentCallCount).IsEqualTo(0);
		await Assert.That(added.Select(static favourite => favourite.Media?.Name ?? string.Empty))
					.IsEquivalentTo(entries.Select(static entry => $"Canonical Anime {entry.Id}"));
	}

	[Test]
	public async Task AddedAndRemovedFavouritesOfATickShareTheSameBatchedRequest()
	{
		var client = new FakeShikiFavouriteClient { FavouritesInfo = new() { Animes = [AnimeMedia(AddedAnimeId), AnimeMedia(RemovedAnimeId),], }, };

		var (added, removed) = await EnrichAsync(client, [AnimeEntry(AddedAnimeId),], [AnimeEntry(RemovedAnimeId),]);

		await Assert.That(client.FavouritesInfoCalls).Count().IsEqualTo(1);
		await Assert.That(client.FavouritesInfoCalls[0].AnimeIds).IsEquivalentTo([AddedAnimeId, RemovedAnimeId,]);
		await Assert.That(added.Single().Media?.Name).IsEqualTo($"Canonical Anime {AddedAnimeId}");
		await Assert.That(removed.Single().Media?.Name).IsEqualTo($"Canonical Anime {RemovedAnimeId}");
	}

	[Test]
	public async Task MoreIdsOfOneKindThanABatchHoldsReachTheClientWhole()
	{
		var entries = Enumerable.Range(1, MoreIdsThanOneBatchHolds).Select(static i => AnimeEntry((uint)i)).ToArray();
		var client = new FakeShikiFavouriteClient();

		await EnrichAsync(client, entries, []);

		await Assert.That(client.FavouritesInfoCalls).Count().IsEqualTo(1);
		await Assert.That(client.FavouritesInfoCalls[0].AnimeIds).IsEquivalentTo(entries.Select(static entry => entry.Id));
	}

	[Test]
	public async Task ACharacterFavouriteCostsExactlyOneEnrichmentCall()
	{
		var client = new FakeShikiFavouriteClient
		{
			FavouritesInfo = new() { Characters = [Character(),], },
			CharacterDetails = new() { Animes = [RelatedMedia(WorksWorkName, score: 9.1f), RelatedMedia("Lesser Work", score: 5),], },
		};

		var favourite = await EnrichOneAsync(client, CharacterEntry());

		await Assert.That(client.EnrichmentCallCount).IsEqualTo(1);
		await Assert.That(client.CharacterDetailsCalls).IsEquivalentTo([CharacterId,]);
		await Assert.That(favourite.BestKnownWork?.Name).IsEqualTo(WorksWorkName);
	}

	[Test]
	public async Task APersonFavouriteCostsExactlyOneEnrichmentCall()
	{
		var client = new FakeShikiFavouriteClient
		{
			FavouritesInfo = new() { People = [Person(isSeyu: false),], },
			PersonDetails = PersonDetails(),
		};

		var favourite = await EnrichOneAsync(client, PersonEntry());

		await Assert.That(client.EnrichmentCallCount).IsEqualTo(1);
		await Assert.That(client.PersonDetailsCalls).IsEquivalentTo([PersonId,]);
		await Assert.That(favourite.BestKnownWork?.Name).IsEqualTo(WorksWorkName);
	}

	[Test]
	public async Task ASeyuTakesItsBestKnownWorkFromRolesRatherThanWorks()
	{
		var client = new FakeShikiFavouriteClient { FavouritesInfo = new() { People = [Person(isSeyu: true),], }, PersonDetails = PersonDetails(), };

		var favourite = await EnrichOneAsync(client, PersonEntry());

		await Assert.That(client.PersonDetailsCalls).IsEquivalentTo([PersonId,]);
		await Assert.That(favourite.BestKnownWork?.Name).IsEqualTo(RolesWorkName);
	}

	[Test]
	public async Task AThrownCharacterEnrichmentDropsOnlyTheBestKnownWorkAndWarns()
	{
		var client = new FakeShikiFavouriteClient
		{
			FavouritesInfo = new() { Characters = [Character(),], },
			CharacterDetailsException = new HttpRequestException("boom"),
		};
		var logger = new RecordingLogger<ShikiUpdateProvider>();

		var favourite = await EnrichOneAsync(client, CharacterEntry(), logger);

		await Assert.That(favourite.Character?.Name).IsEqualTo("Fav Character");
		await Assert.That(favourite.BestKnownWork).IsNull();
		await AssertWarnedAsync(logger);
	}

	[Test]
	public async Task AnEmptyCharacterEnrichmentDropsOnlyTheBestKnownWork()
	{
		var client = new FakeShikiFavouriteClient { FavouritesInfo = new() { Characters = [Character(),], }, CharacterDetails = new(), };
		var logger = new RecordingLogger<ShikiUpdateProvider>();

		var favourite = await EnrichOneAsync(client, CharacterEntry(), logger);

		await Assert.That(favourite.Character?.Name).IsEqualTo("Fav Character");
		await Assert.That(favourite.BestKnownWork).IsNull();
		await Assert.That(logger.Entries).IsEmpty();
	}

	[Test]
	public async Task AThrownPersonEnrichmentDropsOnlyTheBestKnownWorkAndWarns()
	{
		var client = new FakeShikiFavouriteClient
		{
			FavouritesInfo = new() { People = [Person(isSeyu: false),], },
			PersonDetailsException = new HttpRequestException("boom"),
		};
		var logger = new RecordingLogger<ShikiUpdateProvider>();

		var favourite = await EnrichOneAsync(client, PersonEntry(), logger);

		await Assert.That(favourite.Person?.Name).IsEqualTo(PersonName);
		await Assert.That(favourite.BestKnownWork).IsNull();
		await AssertWarnedAsync(logger);
	}

	[Test]
	public async Task AThrownSeyuEnrichmentDropsOnlyTheBestKnownWorkAndWarns()
	{
		var client = new FakeShikiFavouriteClient
		{
			FavouritesInfo = new() { People = [Person(isSeyu: true),], },
			PersonDetailsException = new HttpRequestException("boom"),
		};
		var logger = new RecordingLogger<ShikiUpdateProvider>();

		var favourite = await EnrichOneAsync(client, PersonEntry(), logger);

		await Assert.That(favourite.Person?.IsSeyu).IsTrue();
		await Assert.That(favourite.BestKnownWork).IsNull();
		await AssertWarnedAsync(logger);
	}

	[Test]
	public async Task AnEmptyPersonEnrichmentDropsOnlyTheBestKnownWork()
	{
		var client = new FakeShikiFavouriteClient { FavouritesInfo = new() { People = [Person(isSeyu: false),], }, PersonDetails = new(), };
		var logger = new RecordingLogger<ShikiUpdateProvider>();

		var favourite = await EnrichOneAsync(client, PersonEntry(), logger);

		await Assert.That(favourite.Person?.Name).IsEqualTo(PersonName);
		await Assert.That(favourite.BestKnownWork).IsNull();
		await Assert.That(logger.Entries).IsEmpty();
	}

	[Test]
	public async Task ASeyuWithoutRolesDropsOnlyTheBestKnownWork()
	{
		var client = new FakeShikiFavouriteClient
		{
			FavouritesInfo = new() { People = [Person(isSeyu: true),], },
			PersonDetails = new() { Works = [new() { Anime = RelatedMedia(WorksWorkName, score: 8.5f), },], },
		};
		var logger = new RecordingLogger<ShikiUpdateProvider>();

		var favourite = await EnrichOneAsync(client, PersonEntry(), logger);

		await Assert.That(favourite.Person?.Name).IsEqualTo(PersonName);
		await Assert.That(favourite.BestKnownWork).IsNull();
		await Assert.That(logger.Entries).IsEmpty();
	}

	private static async Task AssertWarnedAsync(RecordingLogger<ShikiUpdateProvider> logger)
	{
		var entry = logger.Single();

		await Assert.That(entry.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(entry.EventId.Name).IsEqualTo("FailedToEnrichFavourite");
	}

	private static async Task<EnrichedFavourite> EnrichOneAsync(FakeShikiFavouriteClient client, FavouriteEntry entry,
																RecordingLogger<ShikiUpdateProvider>? logger = null)
	{
		var (added, _) = await EnrichAsync(client, [entry,], [], logger);

		return added.Single();
	}

	private static Task<(IReadOnlyList<EnrichedFavourite> AddedValues, IReadOnlyList<EnrichedFavourite> RemovedValues)> EnrichAsync(
		FakeShikiFavouriteClient client, IReadOnlyList<FavouriteEntry> added, IReadOnlyList<FavouriteEntry> removed,
		RecordingLogger<ShikiUpdateProvider>? logger = null) =>
		ShikiFavouriteEnrichment.EnrichAsync(added, removed, ShikiUserFeatures.Default, client, logger ?? new(), CancellationToken.None);

	private static FavouriteEntry AnimeEntry(uint id) => new()
	{
		Id = id,
		Name = $"Stored Anime {id}",
		GenericType = "animes",
		SpecificType = "Anime",
	};

	private static FavouriteEntry CharacterEntry() => new()
	{
		Id = CharacterId,
		Name = "Stored Character",
		GenericType = "characters",
		SpecificType = "Character",
	};

	private static FavouriteEntry PersonEntry() => new()
	{
		Id = PersonId,
		Name = "Stored Person",
		GenericType = "people",
		SpecificType = "Person",
	};

	private static AnimeMedia AnimeMedia(uint id) => new()
	{
		Id = id,
		Name = $"Canonical Anime {id}",
		Kind = "tv",
		Status = "released",
		Url = $"https://shikimori.one/animes/{id}",
	};

	private static FavouriteCharacter Character() => new()
	{
		Id = CharacterId,
		Name = "Fav Character",
		Url = $"https://shikimori.one/characters/{CharacterId}",
	};

	private static FavouritePerson Person(bool isSeyu) => new()
	{
		Id = PersonId,
		Name = PersonName,
		IsSeyu = isSeyu,
		Url = $"https://shikimori.one/people/{PersonId}",
	};

	private static PersonDetails PersonDetails() => new()
	{
		Works = [new() { Anime = RelatedMedia(WorksWorkName, score: 8.5f), },],
		Roles = [new() { Animes = [RelatedMedia(RolesWorkName, score: 9.3f),], },],
	};

	private static RelatedMedia RelatedMedia(string name, float score) => new()
	{
		Id = 1,
		Name = name,
		Score = score,
		Url = "https://shikimori.one/animes/1",
	};
}
