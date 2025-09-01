// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.GraphQL;

namespace PaperMalKing.AniList.Wrapper.Tests;

public class RequestsTests
{
	[Test]
	[Arguments(RequestOptions.AnimeList | RequestOptions.MangaList)]
	[Arguments(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.Mangaka)]
	[Arguments(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.CustomLists)]
	[Arguments(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.Favourites)]
	[Arguments(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.Director)]
	[Arguments(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.MediaFormat)]
	[Arguments(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.MediaStatus)]
	[Arguments(RequestOptions.All)]
	public async Task GraphQlRequestBuilderProducesExpectedResult(RequestOptions options)
	{
		var verifySettings = new VerifySettings();
		verifySettings.UseParameters(options);
		var request = Requests.CheckForUpdatesRequest(1u, 1, TimeProvider.System.GetUtcNow().ToUnixTimeSeconds(), 1, 1, options);
		await Verify(request.Query, verifySettings);
	}

	[Test]
	public async Task ProfileRequestProducesExpectedResult()
	{
		await Verify(Requests.GetUserInitialInfoByUsernameRequest("N0D4N", 1).Query);
	}

	[Test]
	public async Task FavoritesInfoRequestReturnsExpectedResult()
	{
		var ids = new[] { 1u };
		await Verify(Requests.FavouritesInfoRequest(1, ids, ids, ids, ids, ids, RequestOptions.All).Query);
	}
}