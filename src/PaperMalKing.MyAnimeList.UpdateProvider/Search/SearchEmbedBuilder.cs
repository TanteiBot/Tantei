// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class SearchEmbedBuilder
{
	private const int DescriptionLimit = 4096;
	private const int FieldValueLimit = 1024;
	private const int MaxDemographicItems = 3;
	private const int MaxListedItems = 7;
	private const int SynopsisLimit = 500;
	private const int TitleLimit = 256;
	private const int UrlLimit = 2048;

	public static Task<DiscordEmbedBuilder> BuildAsync<TMediaType, TStatus>(
		BaseSearchResult<TMediaType, TStatus> result,
		MalUserFeatures features,
		IMyAnimeListEnrichment enrichment,
		string requesterDisplayName,
		string? avatarUrl,
		CancellationToken cancellationToken)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		ArgumentNullException.ThrowIfNull(result);
		ArgumentNullException.ThrowIfNull(enrichment);
		return BuildCoreAsync(result, features, enrichment, requesterDisplayName, avatarUrl, cancellationToken);
	}

	private static async Task<DiscordEmbedBuilder> BuildCoreAsync<TMediaType, TStatus>(
		BaseSearchResult<TMediaType, TStatus> result,
		MalUserFeatures features,
		IMyAnimeListEnrichment enrichment,
		string requesterDisplayName,
		string? avatarUrl,
		CancellationToken cancellationToken)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		var builder = Build(result, features, requesterDisplayName, avatarUrl);

		var isAnime = result is AnimeSearchResult;
		if (features.HasAnyFlag(MalUserFeatures.Themes, MalUserFeatures.Demographic))
		{
			var mediaInfo = isAnime
				? await enrichment.GetAnimeDetailsAsync(result.Id, cancellationToken).ConfigureAwait(false)
				: await enrichment.GetMangaDetailsAsync(result.Id, cancellationToken).ConfigureAwait(false);
			AddMediaInfoFields(builder, mediaInfo, features);
		}

		if (isAnime && features.HasFlag(MalUserFeatures.Seiyu))
		{
			var seiyu = await enrichment.GetAnimeSeiyuAsync(result.Id, cancellationToken).ConfigureAwait(false);
			AddSeiyuField(builder, seiyu);
		}

		return builder;
	}

	public static DiscordEmbedBuilder Build<TMediaType, TStatus>(
		BaseSearchResult<TMediaType, TStatus> result,
		MalUserFeatures features,
		string requesterDisplayName,
		string? avatarUrl)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		ArgumentNullException.ThrowIfNull(result);
		string mediaPath;
		string? total;
		string? season;
		string? studios;
		string? mangakas;
		switch (result)
		{
			case AnimeSearchResult anime:
				mediaPath = "anime";
				total = anime.Episodes == 0U ? null : $"{anime.Episodes.ToString(CultureInfo.InvariantCulture)} ep.";
				season = anime.StartSeason is { Season: not AnimeSeason.Unknown, Year: not 0U } startSeason
					? $"{startSeason.Season.Humanize(LetterCasing.Sentence)} {startSeason.Year.ToString(CultureInfo.InvariantCulture)}"
					: null;
				studios = FormatStudios(anime.Studios);
				mangakas = null;
				break;
			case MangaSearchResult manga:
				mediaPath = "manga";
				total = FormatMangaTotal(manga);
				season = null;
				studios = null;
				mangakas = FormatAuthors(manga.Authors);
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
			features,
			mediaType,
			status,
			total,
			season,
			studios,
			mangakas,
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

	private static string? FormatStudios(IReadOnlyList<Studio>? studios) =>
		studios is not { Count: > 0 }
			? null
			: string.Join(", ", studios.Take(MaxListedItems).Select(static studio => Formatter.MaskedUrl(studio.Name, new Uri(studio.Url))));

	private static string? FormatAuthors(IReadOnlyList<Author>? authors) =>
		authors is not { Count: > 0 }
			? null
			: string.Join(", ", authors.Take(MaxListedItems).Select(static author => Formatter.MaskedUrl(FormatAuthorName(author), new Uri(author.Person.Url))));

	private static string FormatAuthorName(Author author)
	{
		var person = author.Person;
		var name = string.IsNullOrWhiteSpace(person.LastName) ? person.FirstName : $"{person.LastName}, {person.FirstName}";
		return $"{name} ({author.Role})";
	}

	private static void AddMediaInfoFields(DiscordEmbedBuilder builder, MediaInfo mediaInfo, MalUserFeatures features)
	{
		if (features.HasFlag(MalUserFeatures.Themes) && mediaInfo.Themes is not [])
		{
			builder.AddFieldIfPresent("Themes", string.Join(", ", mediaInfo.Themes.Take(MaxListedItems)).Truncate(FieldValueLimit), inline: true);
		}

		if (features.HasFlag(MalUserFeatures.Demographic) && mediaInfo.Demographic is not [])
		{
			builder.AddFieldIfPresent("Demographic", string.Join(", ", mediaInfo.Demographic.Take(MaxDemographicItems)).Truncate(FieldValueLimit), inline: true);
		}
	}

	private static void AddSeiyuField(DiscordEmbedBuilder builder, IReadOnlyList<SeyuInfo> seiyu)
	{
		if (seiyu is [])
		{
			return;
		}

		var text = string.Join(", ", seiyu.Take(MaxListedItems).Select(static info => Formatter.MaskedUrl(info.Name, new Uri(info.Url))));
		builder.AddFieldIfPresent("Seiyu", text.Truncate(FieldValueLimit), inline: true);
	}

	private static DiscordEmbedBuilder Build(
		BaseSearchResult result,
		Uri mediaUrl,
		MalUserFeatures features,
		string? mediaType,
		string? status,
		string? total,
		string? season,
		string? studios,
		string? mangakas,
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
			if (features.HasFlag(MalUserFeatures.Synopsis))
			{
				var synopsis = result.Synopsis.RemoveSourceTail().Trim().Truncate(SynopsisLimit);
				if (!string.IsNullOrWhiteSpace(synopsis))
				{
					builder.WithDescription(synopsis);
				}
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

		builder.AddFieldIfPresent("Type", features.HasFlag(MalUserFeatures.MediaFormat) ? mediaType : null, inline: true);
		builder.AddFieldIfPresent("Status", features.HasFlag(MalUserFeatures.MediaStatus) ? status : null, inline: true);
		builder.AddFieldIfPresent("Score", result.Mean?.ToString("0.##", CultureInfo.InvariantCulture), inline: true);
		builder.AddFieldIfPresent("Total", total, inline: true);
		builder.AddFieldIfPresent("Season", season, inline: true);
		builder.AddFieldIfPresent("Members", result.ListUserCount.ToString("N0", CultureInfo.InvariantCulture), inline: true);

		if (features.HasFlag(MalUserFeatures.Studio))
		{
			builder.AddFieldIfPresent("Studios", studios?.Truncate(FieldValueLimit), inline: true);
		}

		if (features.HasFlag(MalUserFeatures.Mangakas))
		{
			builder.AddFieldIfPresent("Authors", mangakas?.Truncate(FieldValueLimit), inline: true);
		}

		if (features.HasFlag(MalUserFeatures.Genres))
		{
			var genres = string.Join(
				", ",
				(result.Genres ?? []).Select(static genre => genre.Name).Where(static name => !string.IsNullOrWhiteSpace(name)).Take(MaxListedItems));
			builder.AddFieldIfPresent("Genres", genres.Truncate(FieldValueLimit));
		}

		return builder;
	}

	private static bool IsValidUrl([NotNullWhen(true)] string? url) => !string.IsNullOrWhiteSpace(url) && url.Length <= UrlLimit;
}
