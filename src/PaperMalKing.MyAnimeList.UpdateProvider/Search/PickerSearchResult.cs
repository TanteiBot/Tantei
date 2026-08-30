// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal abstract class PickerSearchResult
{
	public abstract MatchRank Rank { get; }

	public abstract PickerMediaKind MediaKind { get; }

	public abstract uint Id { get; }

	public abstract string PrimaryTitle { get; }

	public abstract string MediaType { get; }

	public abstract uint? Year { get; }

	public abstract double? Mean { get; }

	public abstract uint ListUserCount { get; }

	public static PickerSearchResult<AnimeSearchResult> ForAnime(RankedSearchResult<AnimeSearchResult> result) => new(
		result,
		PickerMediaKind.Anime,
		static anime => anime.MediaType.Humanize(LetterCasing.Sentence),
		static anime => anime.StartSeason is { Year: not 0U } startSeason ? startSeason.Year : null,
		static (anime, context) => SearchEmbedBuilder.Build(anime, context.RequesterDisplayName, context.RequesterAvatarUrl));

	public static PickerSearchResult<MangaSearchResult> ForManga(RankedSearchResult<MangaSearchResult> result) => new(
		result,
		PickerMediaKind.Manga,
		static manga => manga.MediaType.Humanize(LetterCasing.Sentence),
		static _ => null,
		static (manga, context) => SearchEmbedBuilder.Build(manga, context.RequesterDisplayName, context.RequesterAvatarUrl));

	public abstract DiscordEmbedBuilder BuildEmbed(PickerSearchContext context);
}

internal sealed class PickerSearchResult<TResult> : PickerSearchResult
	where TResult : BaseSearchResult
{
	private readonly TResult _result;
	private readonly Func<TResult, string> _getMediaType;
	private readonly Func<TResult, uint?> _getYear;
	private readonly Func<TResult, PickerSearchContext, DiscordEmbedBuilder> _buildEmbed;

	public override MatchRank Rank { get; }

	public override PickerMediaKind MediaKind { get; }

	public override uint Id => this._result.Id;

	public override string PrimaryTitle => this._result.PrimaryTitle;

	public override string MediaType => this._getMediaType(this._result);

	public override uint? Year => this._getYear(this._result);

	public override double? Mean => this._result.Mean;

	public override uint ListUserCount => this._result.ListUserCount;

	internal PickerSearchResult(
		RankedSearchResult<TResult> result,
		PickerMediaKind mediaKind,
		Func<TResult, string> getMediaType,
		Func<TResult, uint?> getYear,
		Func<TResult, PickerSearchContext, DiscordEmbedBuilder> buildEmbed)
	{
		this._result = result.Result;
		this.Rank = result.Rank;
		this.MediaKind = mediaKind;
		this._getMediaType = getMediaType;
		this._getYear = getYear;
		this._buildEmbed = buildEmbed;
	}

	public override DiscordEmbedBuilder BuildEmbed(PickerSearchContext context) => this._buildEmbed(this._result, context);
}
