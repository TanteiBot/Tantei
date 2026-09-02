// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using DiscordEmbedBuilder = DSharpPlus.Entities.DiscordEmbedBuilder;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests;

public sealed class UpdateEnrichmentEmbedTests
{
	private const uint AnimeId = 101;

	private const uint MangaId = 202;

	private const string DemographicField = "Demographic";

	private const string FirstPersonUrl = "https://myanimelist.net/people/1";

	private const string SeiyuField = "Seiyu";

	private const string ThemesField = "Themes";

	[Test]
	public async Task AnimeEnrichmentPreservesLabelsLimitsOrderDuplicatesAndMaskedLinks()
	{
		var client = new FakeMyAnimeListEnrichmentClient
		{
			AnimeDetailsResult = new()
			{
				Themes = ["Action", "Drama", "Drama", "Mystery", "Sports", "Music", "School", "Excluded Theme",],
				Demographic = ["Shounen", "Seinen", "Shounen", "Excluded Demographic",],
			},
			AnimeSeiyuResult =
			[
				new() { Name = "Voice One", Url = FirstPersonUrl, },
				new() { Name = "Voice Two", Url = "https://myanimelist.net/people/2", },
				new() { Name = "Voice One", Url = FirstPersonUrl, },
				new() { Name = "Voice Four", Url = "https://myanimelist.net/people/4", },
				new() { Name = "Voice Five", Url = "https://myanimelist.net/people/5", },
				new() { Name = "Voice Six", Url = "https://myanimelist.net/people/6", },
				new() { Name = "Voice Seven", Url = "https://myanimelist.net/people/7", },
				new() { Name = "Excluded Voice", Url = "https://myanimelist.net/people/8", },
			],
		};

		var embed = await BuildAnimeEmbedAsync(client,
			MalUserFeatures.Themes | MalUserFeatures.Demographic | MalUserFeatures.Seiyu, CancellationToken.None);

		await Assert.That(EnrichmentFieldNames(embed)).IsEquivalentTo([ThemesField, DemographicField, SeiyuField,]);
		await Assert.That(FieldValue(embed, ThemesField)).IsEqualTo("Action, Drama, Drama, Mystery, Sports, Music, School");
		await Assert.That(FieldValue(embed, DemographicField)).IsEqualTo("Shounen, Seinen, Shounen");
		const string expectedSeiyu = $"[Voice One]({FirstPersonUrl}), [Voice Two](https://myanimelist.net/people/2), " +
			$"[Voice One]({FirstPersonUrl}), [Voice Four](https://myanimelist.net/people/4), " +
			"[Voice Five](https://myanimelist.net/people/5), [Voice Six](https://myanimelist.net/people/6), " +
			"[Voice Seven](https://myanimelist.net/people/7)";
		await Assert.That(FieldValue(embed, SeiyuField)).IsEqualTo(expectedSeiyu);
	}

	[Test]
	[Arguments(MalUserFeatures.Themes, ThemesField)]
	[Arguments(MalUserFeatures.Demographic, DemographicField)]
	[Arguments(MalUserFeatures.Seiyu, SeiyuField)]
	public async Task EachFeatureGateIndependentlyControlsRequestsAndDisplay(MalUserFeatures feature, string expectedField)
	{
		var client = PopulatedClient();

		var embed = await BuildAnimeEmbedAsync(client, feature, CancellationToken.None);

		await Assert.That(EnrichmentFieldNames(embed)).IsEquivalentTo([expectedField,]);
		await Assert.That(client.AnimeDetailsCalls).Count().IsEqualTo(feature is MalUserFeatures.Themes or MalUserFeatures.Demographic ? 1 : 0);
		await Assert.That(client.AnimeSeiyuCalls).Count().IsEqualTo(feature is MalUserFeatures.Seiyu ? 1 : 0);
	}

	[Test]
	public async Task DisabledEnrichmentFeaturesMakeNoRequestsAndDisplayNoFields()
	{
		var client = PopulatedClient();

		var embed = await BuildAnimeEmbedAsync(client, MalUserFeatures.None, CancellationToken.None);

		await Assert.That(EnrichmentFieldNames(embed)).IsEmpty();
		await Assert.That(client.AnimeDetailsCalls).IsEmpty();
		await Assert.That(client.MangaDetailsCalls).IsEmpty();
		await Assert.That(client.AnimeSeiyuCalls).IsEmpty();
	}

	[Test]
	public async Task AnimeUpdateRequestsAnimeDetails()
	{
		var client = PopulatedClient();

		await BuildAnimeEmbedAsync(client, MalUserFeatures.Themes, CancellationToken.None);

		await Assert.That(client.AnimeDetailsCalls.Select(static call => call.Id)).IsEquivalentTo([(long)AnimeId,]);
		await Assert.That(client.MangaDetailsCalls).IsEmpty();
	}

	[Test]
	public async Task MangaUpdateRequestsMangaDetailsAndNeverRequestsSeiyu()
	{
		var client = PopulatedClient();

		var embed = await BuildMangaEmbedAsync(client, MalUserFeatures.Themes | MalUserFeatures.Seiyu, CancellationToken.None);

		await Assert.That(client.MangaDetailsCalls.Select(static call => call.Id)).IsEquivalentTo([(long)MangaId,]);
		await Assert.That(client.AnimeDetailsCalls).IsEmpty();
		await Assert.That(client.AnimeSeiyuCalls).IsEmpty();
		await Assert.That(EnrichmentFieldNames(embed)).IsEquivalentTo([ThemesField,]);
	}

	[Test]
	public async Task UnavailableDetailsOmitOnlyDetailsFields()
	{
		var client = new FakeMyAnimeListEnrichmentClient
		{
			AnimeDetailsResult = MediaInfo.Empty,
			AnimeSeiyuResult = [new() { Name = "Voice", Url = FirstPersonUrl, },],
		};

		var embed = await BuildAnimeEmbedAsync(client,
			MalUserFeatures.Themes | MalUserFeatures.Demographic | MalUserFeatures.Seiyu, CancellationToken.None);

		await Assert.That(EnrichmentFieldNames(embed)).IsEquivalentTo([SeiyuField,]);
		await Assert.That(client.AnimeDetailsCalls).Count().IsEqualTo(1);
		await Assert.That(client.AnimeSeiyuCalls).Count().IsEqualTo(1);
	}

	[Test]
	[Arguments(true, DemographicField)]
	[Arguments(false, ThemesField)]
	public async Task EmptyDetailsCollectionOmitsOnlyItsOwnField(bool themesAreEmpty, string expectedField)
	{
		var client = new FakeMyAnimeListEnrichmentClient
		{
			AnimeDetailsResult = new()
			{
				Themes = themesAreEmpty ? [] : ["Theme",],
				Demographic = themesAreEmpty ? [DemographicField,] : [],
			},
		};

		var embed = await BuildAnimeEmbedAsync(client,
			MalUserFeatures.Themes | MalUserFeatures.Demographic, CancellationToken.None);

		await Assert.That(EnrichmentFieldNames(embed)).IsEquivalentTo([expectedField,]);
	}

	[Test]
	public async Task UnavailableCharactersOmitOnlySeiyuField()
	{
		var client = new FakeMyAnimeListEnrichmentClient
		{
			AnimeDetailsResult = new() { Themes = ["Theme",], Demographic = ["Demographic",], },
			AnimeSeiyuResult = [],
		};

		var embed = await BuildAnimeEmbedAsync(client,
			MalUserFeatures.Themes | MalUserFeatures.Demographic | MalUserFeatures.Seiyu, CancellationToken.None);

		await Assert.That(EnrichmentFieldNames(embed)).IsEquivalentTo([ThemesField, DemographicField,]);
		await Assert.That(client.AnimeDetailsCalls).Count().IsEqualTo(1);
		await Assert.That(client.AnimeSeiyuCalls).Count().IsEqualTo(1);
	}

	[Test]
	[Arguments(MalUserFeatures.Themes)]
	[Arguments(MalUserFeatures.Seiyu)]
	public async Task CallerCancellationPropagatesUnchanged(MalUserFeatures feature)
	{
		using var cancellationSource = new CancellationTokenSource();
		await cancellationSource.CancelAsync();
		var expected = new OperationCanceledException(cancellationSource.Token);
		var client = feature is MalUserFeatures.Themes
			? new FakeMyAnimeListEnrichmentClient { AnimeDetailsCancellation = expected, }
			: new FakeMyAnimeListEnrichmentClient { AnimeSeiyuCancellation = expected, };
		OperationCanceledException? actual = null;

		try
		{
			await BuildAnimeEmbedAsync(client, feature, cancellationSource.Token);
		}
		catch (OperationCanceledException exception)
		{
			actual = exception;
		}

		var recordedToken = feature is MalUserFeatures.Themes
			? client.AnimeDetailsCalls.Single().CancellationToken
			: client.AnimeSeiyuCalls.Single().CancellationToken;
		await Assert.That(recordedToken).IsEqualTo(cancellationSource.Token);
		await Assert.That(actual?.CancellationToken).IsEqualTo(cancellationSource.Token);
		await Assert.That(ReferenceEquals(actual, expected)).IsTrue();
	}

	private static FakeMyAnimeListEnrichmentClient PopulatedClient() => new()
	{
		AnimeDetailsResult = new() { Themes = ["Theme",], Demographic = [DemographicField,], },
		MangaDetailsResult = new() { Themes = ["Manga Theme",], Demographic = ["Manga Demographic",], },
		AnimeSeiyuResult = [new() { Name = "Voice", Url = FirstPersonUrl, },],
	};

	private static IEnumerable<string> EnrichmentFieldNames(DiscordEmbedBuilder embed) =>
		embed.Fields.Where(static field => string.Equals(field.Name, ThemesField, StringComparison.Ordinal) ||
			string.Equals(field.Name, DemographicField, StringComparison.Ordinal) || string.Equals(field.Name, SeiyuField, StringComparison.Ordinal))
			.Select(static field => field.Name);

	private static string FieldValue(DiscordEmbedBuilder embed, string name) =>
		embed.Fields.Single(field => string.Equals(field.Name, name, StringComparison.Ordinal)).Value;

	private static Task<DiscordEmbedBuilder> BuildAnimeEmbedAsync(
		FakeMyAnimeListEnrichmentClient client, MalUserFeatures features, CancellationToken cancellationToken) =>
		CreateAnimeEntry().ToDiscordEmbedBuilderAsync<AnimeListEntry, AnimeListEntryNode, AnimeListEntryStatus, AnimeMediaType,
			AnimeAiringStatus, AnimeListStatus>(CreateUser(), client, CreateMalUser(features), cancellationToken);

	private static Task<DiscordEmbedBuilder> BuildMangaEmbedAsync(
		FakeMyAnimeListEnrichmentClient client, MalUserFeatures features, CancellationToken cancellationToken) =>
		CreateMangaEntry().ToDiscordEmbedBuilderAsync<MangaListEntry, MangaListEntryNode, MangaListEntryStatus, MangaMediaType,
			MangaPublishingStatus, MangaListStatus>(CreateUser(), client, CreateMalUser(features), cancellationToken);

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

	private static AnimeListEntry CreateAnimeEntry() => new()
	{
		Node = new()
		{
			Id = AnimeId,
			Title = "Anime",
			Episodes = 1,
			MediaType = AnimeMediaType.TV,
			Status = AnimeAiringStatus.CurrentlyAiring,
		},
		Status = new()
		{
			Status = AnimeListStatus.Watching,
			Score = 0,
			UpdatedAt = DateTimeOffset.UnixEpoch,
			EpisodesWatched = 1,
			IsRewatching = false,
			TimesRewatched = 0,
		},
	};

	private static MangaListEntry CreateMangaEntry() => new()
	{
		Node = new()
		{
			Id = MangaId,
			Title = "Manga",
			TotalVolumes = 1,
			TotalChapters = 1,
			MediaType = MangaMediaType.Manga,
			Status = MangaPublishingStatus.CurrentlyPublishing,
		},
		Status = new()
		{
			Status = MangaListStatus.Reading,
			Score = 0,
			UpdatedAt = DateTimeOffset.UnixEpoch,
			VolumesRead = 1,
			ChaptersRead = 1,
			IsRereading = false,
			TimesReread = 0,
		},
	};
}
