// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class SearchEmbedBuilder
{
	private const int AuthorNameLimit = 256;
	private const int DescriptionLimit = 4096;
	private const int FieldValueLimit = 1024;
	private const int SynopsisLimit = 500;
	private const int TitleLimit = 256;
	private const int UrlLimit = 2048;

	public static DiscordEmbedBuilder Build(AnimeSearchResult result, string requesterDisplayName, string? avatarUrl)
	{
		ArgumentNullException.ThrowIfNull(result);
		var total = result.Episodes == 0U ? null : $"{result.Episodes.ToString(CultureInfo.InvariantCulture)} ep.";
		var season = result.StartSeason is { Season: not AnimeSeason.Unknown, Year: not 0U } startSeason
			? $"{startSeason.Season.Humanize(LetterCasing.Sentence)} {startSeason.Year.ToString(CultureInfo.InvariantCulture)}"
			: null;
		return Build(
			result,
			new($"https://myanimelist.net/anime/{result.Id.ToString(CultureInfo.InvariantCulture)}"),
			result.MediaType == AnimeMediaType.Unknown ? null : result.MediaType.Humanize(LetterCasing.Sentence),
			result.Status == AnimeAiringStatus.Unknown ? null : result.Status.Humanize(LetterCasing.Sentence),
			total,
			season,
			requesterDisplayName,
			avatarUrl);
	}

	public static DiscordEmbedBuilder Build(MangaSearchResult result, string requesterDisplayName, string? avatarUrl)
	{
		ArgumentNullException.ThrowIfNull(result);
		var totals = new List<string>(2);
		if (result.Chapters != 0U)
		{
			totals.Add($"{result.Chapters.ToString(CultureInfo.InvariantCulture)} ch");
		}

		if (result.Volumes != 0U)
		{
			totals.Add($"{result.Volumes.ToString(CultureInfo.InvariantCulture)} v.");
		}

		return Build(
			result,
			new($"https://myanimelist.net/manga/{result.Id.ToString(CultureInfo.InvariantCulture)}"),
			result.MediaType == MangaMediaType.Unknown ? null : result.MediaType.Humanize(LetterCasing.Sentence),
			result.Status == MangaPublishingStatus.Unknown ? null : result.Status.Humanize(LetterCasing.Sentence),
			totals.Count == 0 ? null : string.Join(", ", totals),
			season: null,
			requesterDisplayName,
			avatarUrl);
	}

	private static DiscordEmbedBuilder Build<TMediaType, TStatus>(
		BaseSearchResult<TMediaType, TStatus> result,
		Uri mediaUrl,
		string? mediaType,
		string? status,
		string? total,
		string? season,
		string requesterDisplayName,
		string? avatarUrl)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(requesterDisplayName);
		var builder = new DiscordEmbedBuilder()
			.WithColor(Constants.MalBlue)
			.WithMalUpdateProviderFooter();
		builder.Author = new()
		{
			Name = $"Requested by {requesterDisplayName}".Truncate(AuthorNameLimit),
			IconUrl = IsValidUrl(avatarUrl) ? avatarUrl : null,
		};

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

		var genres = string.Join(", ", result.Genres.Select(static genre => genre.Name).Where(static name => !string.IsNullOrWhiteSpace(name)).Take(7));
		builder.AddFieldIfPresent("Genres", genres.Truncate(FieldValueLimit));
		return builder;
	}

	private static bool IsValidUrl([NotNullWhen(true)] string? url) => !string.IsNullOrWhiteSpace(url) && url.Length <= UrlLimit;
}
