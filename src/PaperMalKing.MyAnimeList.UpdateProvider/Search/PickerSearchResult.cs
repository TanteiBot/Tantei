// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using DSharpPlus.Entities;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed record PickerSearchResult
{
	private readonly object _result;

	public MatchRank Rank { get; }

	public PickerMediaKind MediaKind { get; }

	public uint Id => this._result switch
	{
		AnimeSearchResult anime => anime.Id,
		MangaSearchResult manga => manga.Id,
		_ => throw new UnreachableException(),
	};

	public string PrimaryTitle => this._result switch
	{
		AnimeSearchResult anime => anime.PrimaryTitle,
		MangaSearchResult manga => manga.PrimaryTitle,
		_ => throw new UnreachableException(),
	};

	public string MediaType => this._result switch
	{
		AnimeSearchResult anime => anime.MediaType.ToString(),
		MangaSearchResult manga => manga.MediaType.ToString(),
		_ => throw new UnreachableException(),
	};

	public uint? Year => this._result is AnimeSearchResult { StartSeason.Year: not 0U } anime ? anime.StartSeason.Year : null;

	public double? Mean => this._result switch
	{
		AnimeSearchResult anime => anime.Mean,
		MangaSearchResult manga => manga.Mean,
		_ => throw new UnreachableException(),
	};

	public uint ListUserCount => this._result switch
	{
		AnimeSearchResult anime => anime.ListUserCount,
		MangaSearchResult manga => manga.ListUserCount,
		_ => throw new UnreachableException(),
	};

	private PickerSearchResult(object result, MatchRank rank, PickerMediaKind mediaKind)
	{
		this._result = result;
		this.Rank = rank;
		this.MediaKind = mediaKind;
	}

	public static PickerSearchResult ForAnime(RankedSearchResult<AnimeSearchResult> result) => new(result.Result, result.Rank, PickerMediaKind.Anime);

	public static PickerSearchResult ForManga(RankedSearchResult<MangaSearchResult> result) => new(result.Result, result.Rank, PickerMediaKind.Manga);

	public DiscordEmbedBuilder BuildEmbed(PickerSearchContext context) => this._result switch
	{
		AnimeSearchResult anime => SearchEmbedBuilder.Build(anime, context.RequesterDisplayName, context.RequesterAvatarUrl),
		MangaSearchResult manga => SearchEmbedBuilder.Build(manga, context.RequesterDisplayName, context.RequesterAvatarUrl),
		_ => throw new UnreachableException(),
	};
}
