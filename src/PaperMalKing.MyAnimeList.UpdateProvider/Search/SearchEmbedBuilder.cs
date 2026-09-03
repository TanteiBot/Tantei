// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class SearchEmbedBuilder
{
	private const int DescriptionLimit = 4096;
	private const int FieldValueLimit = 1024;
	private const int SynopsisLimit = 500;
	private const int TitleLimit = 256;
	private const int UrlLimit = 2048;

	public static DiscordEmbedBuilder Build<TMediaType, TStatus>(
		BaseSearchResult<TMediaType, TStatus> result,
		string requesterDisplayName,
		string? avatarUrl)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		ArgumentNullException.ThrowIfNull(result);
		string mediaPath;
		string? total;
		string? season;
		switch (result)
		{
			case AnimeSearchResult anime:
				mediaPath = "anime";
				total = anime.Episodes == 0U ? null : $"{anime.Episodes.ToString(CultureInfo.InvariantCulture)} ep.";
				season = anime.StartSeason is { Season: not AnimeSeason.Unknown, Year: not 0U } startSeason
					? $"{startSeason.Season.Humanize(LetterCasing.Sentence)} {startSeason.Year.ToString(CultureInfo.InvariantCulture)}"
					: null;
				break;
			case MangaSearchResult manga:
				mediaPath = "manga";
				total = FormatMangaTotal(manga);
				season = null;
				break;
			default:
				throw new ArgumentException("The MAL Search Result type is not supported.", nameof(result));
		}

		var mediaType = EqualityComparer<TMediaType>.Default.Equals(result.MediaType, default)
			? null
			: result.MediaType.Humanize(LetterCasing.Sentence);
		var status = EqualityComparer<TStatus>.Default.Equals(result.Status, default)
			? null
			: result.Status.Humanize(LetterCasing.Sentence);
		return Build(
			result,
			new($"https://myanimelist.net/{mediaPath}/{result.Id.ToString(CultureInfo.InvariantCulture)}"),
			mediaType,
			status,
			total,
			season,
			requesterDisplayName,
			avatarUrl);
	}

	private static string? FormatMangaTotal(MangaSearchResult manga)
	{
		var totals = new List<string>(2);
		if (manga.Chapters != 0U)
		{
			totals.Add($"{manga.Chapters.ToString(CultureInfo.InvariantCulture)} ch");
		}

		if (manga.Volumes != 0U)
		{
			totals.Add($"{manga.Volumes.ToString(CultureInfo.InvariantCulture)} v.");
		}

		return totals.Count == 0 ? null : string.Join(", ", totals);
	}

	private static DiscordEmbedBuilder Build(
		BaseSearchResult result,
		Uri mediaUrl,
		string? mediaType,
		string? status,
		string? total,
		string? season,
		string requesterDisplayName,
		string? avatarUrl)
	{
		var builder = new DiscordEmbedBuilder()
			.WithColor(Constants.MalBlue)
			.WithMalUpdateProviderFooter();
		builder.WithRequestedByAuthor(requesterDisplayName, avatarUrl);

		if (result.PrimaryTitle.Length <= TitleLimit)
		{
			builder.WithTitle(result.PrimaryTitle).WithUrl(mediaUrl);
			var synopsis = result.Synopsis.RemoveSourceTail().Trim().Truncate(SynopsisLimit);
			if (!string.IsNullOrWhiteSpace(synopsis))
			{
				builder.WithDescription(synopsis);
			}
		}
		else
		{
			var linkedTitle = Formatter.MaskedUrl(result.PrimaryTitle, mediaUrl);
			if (linkedTitle.Length > DescriptionLimit)
			{
				throw new ArgumentException("The linked Primary Title exceeds Discord's description limit.", nameof(result));
			}

			builder.WithDescription(linkedTitle);
		}

		var largePosterUrl = result.Picture?.Large;
		var thumbnailUrl = IsValidUrl(largePosterUrl) ? largePosterUrl : result.Picture?.Medium;
		if (IsValidUrl(thumbnailUrl))
		{
			builder.WithThumbnail(thumbnailUrl);
		}

		builder.AddFieldIfPresent("Type", mediaType, inline: true);
		builder.AddFieldIfPresent("Status", status, inline: true);
		builder.AddFieldIfPresent("Score", result.Mean?.ToString("0.##", CultureInfo.InvariantCulture), inline: true);
		builder.AddFieldIfPresent("Total", total, inline: true);
		builder.AddFieldIfPresent("Season", season, inline: true);
		builder.AddFieldIfPresent("Members", result.ListUserCount.ToString("N0", CultureInfo.InvariantCulture), inline: true);

		var genres = string.Join(
			", ",
			(result.Genres ?? []).Select(static genre => genre.Name).Where(static name => !string.IsNullOrWhiteSpace(name)).Take(7));
		builder.AddFieldIfPresent("Genres", genres.Truncate(FieldValueLimit));
		return builder;
	}

	private static bool IsValidUrl([NotNullWhen(true)] string? url) => !string.IsNullOrWhiteSpace(url) && url.Length <= UrlLimit;
}
