// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.GraphQL;

namespace PaperMalKing.AniList.Wrapper.Tests;

public class RequestsTests
{
	[Theory]
	[InlineData(RequestOptions.AnimeList | RequestOptions.MangaList)]
	[InlineData(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.Mangaka)]
	[InlineData(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.CustomLists)]
	[InlineData(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.Favourites)]
	[InlineData(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.Director)]
	[InlineData(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.MediaFormat)]
	[InlineData(RequestOptions.AnimeList | RequestOptions.MangaList | RequestOptions.MediaStatus)]
	[InlineData(RequestOptions.All)]
	internal Task GraphQlRequestBuilderProducesExpectedResult(RequestOptions options)
	{
		var verifySettings = new VerifySettings();
		verifySettings.UseParameters(options);
		var request = Requests.CheckForUpdatesRequest(1u, 1, TimeProvider.System.GetUtcNow().ToUnixTimeSeconds(), 1, 1, options);
		return Verify(request.Query, verifySettings);
	}

	[Fact]
	public Task ProfileRequestProducesExpectedResult()
	{
		return Verify(Requests.GetUserInitialInfoByUsernameRequest("N0D4N", 1).Query);
	}

	[Fact]
	public Task FavoritesInfoRequestReturnsExpectedResult()
	{
		var ids = new[] { 1u };
		return Verify(Requests.FavouritesInfoRequest(1, ids, ids, ids, ids, ids, RequestOptions.All).Query);
	}
}