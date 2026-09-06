// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus.Entities;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests;

public sealed class MalMediaEmbedsHelperTests
{
	private const string StudiosField = "Studios";
	private const string AuthorsField = "Authors";
	private const string SeiyuField = "Seiyu";
	private const string GenresField = "Genres";
	private const string ThemesField = "Themes";
	private const string DemographicField = "Demographic";
	private const int SevenItemCap = 7;

	[Test]
	public async Task AddStudiosSkipsFieldWhenFeatureDisabled()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddStudios(embed, [Studio(1U, "Madhouse")], MalUserFeatures.None);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddStudiosSkipsFieldWhenNull()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddStudios(embed, studios: null, MalUserFeatures.Studio);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddStudiosSkipsFieldWhenEmpty()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddStudios(embed, [], MalUserFeatures.Studio);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddStudiosCapsAtThree()
	{
		var studios = Enumerable.Range(1, 5).Select(static number => Studio((uint)number, Named("Studio", number))).ToArray();
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddStudios(embed, studios, MalUserFeatures.Studio);

		var field = SingleField(embed, StudiosField);
		await Assert.That(field.Value).IsEqualTo(
			"[Studio 1](https://myanimelist.net/anime/producer/1), [Studio 2](https://myanimelist.net/anime/producer/2), " +
			"[Studio 3](https://myanimelist.net/anime/producer/3)");
		await Assert.That(field.Inline).IsTrue();
	}

	[Test]
	public async Task AddAuthorsSkipsFieldWhenFeatureDisabled()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddAuthors(embed, [Author("Story", "Miura", "Kentarou", 1U)], MalUserFeatures.None);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddAuthorsSkipsFieldWhenNull()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddAuthors(embed, authors: null, MalUserFeatures.Mangakas);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddAuthorsSkipsFieldWhenEmpty()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddAuthors(embed, [], MalUserFeatures.Mangakas);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddAuthorsCapsAtSeven()
	{
		var authors = Enumerable.Range(1, 8).Select(static number => Author("Story", Named("Last", number), Named("First", number), (uint)number)).ToArray();
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddAuthors(embed, authors, MalUserFeatures.Mangakas);

		var field = SingleField(embed, AuthorsField);
		await Assert.That(field.Value.Split(", [", StringSplitOptions.None)).Count().IsEqualTo(SevenItemCap);
	}

	[Test]
	public async Task AddAuthorsRendersLastNameFirstNameAndRole()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddAuthors(embed, [Author("Story & Art", "Miura", "Kentarou", 1U)], MalUserFeatures.Mangakas);

		var field = SingleField(embed, AuthorsField);
		await Assert.That(field.Value).IsEqualTo("[Miura, Kentarou (Story & Art)](https://myanimelist.net/people/1)");
	}

	[Test]
	[Arguments(null)]
	[Arguments("")]
	[Arguments(" ")]
	public async Task AddAuthorsOmitsLastNameWhenBlank(string? lastName)
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddAuthors(embed, [Author("Art", lastName, "Kentarou", 1U)], MalUserFeatures.Mangakas);

		var field = SingleField(embed, AuthorsField);
		await Assert.That(field.Value).IsEqualTo("[Kentarou (Art)](https://myanimelist.net/people/1)");
	}

	[Test]
	public async Task AddSeiyuSkipsFieldWhenFeatureDisabled()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddSeiyu(embed, [Seiyu("Voice", "https://myanimelist.net/people/1")], MalUserFeatures.None);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddSeiyuSkipsFieldWhenEmpty()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddSeiyu(embed, [], MalUserFeatures.Seiyu);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddSeiyuCapsAtSeven()
	{
		var seiyu = Enumerable.Range(1, 8).Select(static number => Seiyu(Named("Voice", number), $"https://myanimelist.net/people/{number.ToString(CultureInfo.InvariantCulture)}")).ToArray();
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddSeiyu(embed, seiyu, MalUserFeatures.Seiyu);

		var field = SingleField(embed, SeiyuField);
		await Assert.That(field.Value.Split(", [", StringSplitOptions.None)).Count().IsEqualTo(SevenItemCap);
	}

	[Test]
	public async Task AddGenresSkipsFieldWhenFeatureDisabled()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddGenres(embed, [Genre("Action")], MalUserFeatures.None);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddGenresSkipsFieldWhenNull()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddGenres(embed, genres: null, MalUserFeatures.Genres);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddGenresSkipsFieldWhenEmpty()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddGenres(embed, [], MalUserFeatures.Genres);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddGenresCapsAtSeven()
	{
		var genres = Enumerable.Range(1, 8).Select(static number => Genre(Named("Genre", number))).ToArray();
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddGenres(embed, genres, MalUserFeatures.Genres);

		var field = SingleField(embed, GenresField);
		await Assert.That(field.Value).IsEqualTo("Genre 1, Genre 2, Genre 3, Genre 4, Genre 5, Genre 6, Genre 7");
	}

	[Test]
	public async Task AddGenresTitleCasesFirstCharacter()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddGenres(embed, [Genre("award winning"), Genre("drama")], MalUserFeatures.Genres);

		var field = SingleField(embed, GenresField);
		await Assert.That(field.Value).IsEqualTo("Award winning, Drama");
	}

	[Test]
	public async Task AddGenresFiltersWhitespaceEntries()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddGenres(embed, [Genre(" "), Genre("mystery"), Genre("")], MalUserFeatures.Genres);

		var field = SingleField(embed, GenresField);
		await Assert.That(field.Value).IsEqualTo("Mystery");
	}

	[Test]
	public async Task AddThemesSkipsFieldWhenFeatureDisabled()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddThemes(embed, new() { Themes = ["Psychological"], Demographic = [] }, MalUserFeatures.None);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddThemesSkipsFieldWhenEmpty()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddThemes(embed, MediaInfo.Empty, MalUserFeatures.Themes);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddThemesCapsAtSeven()
	{
		var themes = Enumerable.Range(1, 8).Select(static number => Named("Theme", number)).ToArray();
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddThemes(embed, new() { Themes = themes, Demographic = [] }, MalUserFeatures.Themes);

		var field = SingleField(embed, ThemesField);
		await Assert.That(field.Value).IsEqualTo("Theme 1, Theme 2, Theme 3, Theme 4, Theme 5, Theme 6, Theme 7");
	}

	[Test]
	public async Task AddDemographicSkipsFieldWhenFeatureDisabled()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddDemographic(embed, new() { Themes = [], Demographic = ["Seinen"] }, MalUserFeatures.None);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddDemographicSkipsFieldWhenEmpty()
	{
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddDemographic(embed, MediaInfo.Empty, MalUserFeatures.Demographic);

		await Assert.That(embed.Fields).IsEmpty();
	}

	[Test]
	public async Task AddDemographicCapsAtThree()
	{
		var demographic = Enumerable.Range(1, 5).Select(static number => Named("Demographic", number)).ToArray();
		var embed = new DiscordEmbedBuilder();

		MalMediaEmbeds.AddDemographic(embed, new() { Themes = [], Demographic = demographic }, MalUserFeatures.Demographic);

		var field = SingleField(embed, DemographicField);
		await Assert.That(field.Value).IsEqualTo("Demographic 1, Demographic 2, Demographic 3");
	}

	private static string Named(string prefix, int number) =>
		string.Create(CultureInfo.InvariantCulture, $"{prefix} {number}");

	private static DiscordEmbedField SingleField(DiscordEmbedBuilder embed, string name) =>
		embed.Fields.Single(field => string.Equals(field.Name, name, StringComparison.Ordinal));

	private static Studio Studio(uint id, string name) => new() { Id = id, Name = name };

	private static Genre Genre(string name) => new() { Name = name };

	private static Author Author(string role, string? lastName, string firstName, uint id) => new()
	{
		Role = role,
		Person = new() { Id = id, FirstName = firstName, LastName = lastName },
	};

	private static SeyuInfo Seiyu(string name, string url) => new() { Name = name, Url = url };
}
