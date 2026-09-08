// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don\'t access instance data should be static", Justification = "False positive")]
[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
internal static class MalMediaEmbeds
{
	private const int DescriptionLimit = 4096;
	private const int SynopsisLimit = 500;
	private const int TitleLimit = 256;

	private static readonly DiscordEmbedBuilder.EmbedFooter MalUpdateFooter = new()
	{
		IconUrl = Constants.FavIcon,
		Text = Constants.Name,
	};

	private static readonly DiscordColor[] Colors =
	[
		DiscordColor.NotQuiteBlack, Constants.MalGreen, Constants.MalBlue, Constants.MalYellow, Constants.MalRed, Constants.MalGrey,
	];

	public static DiscordEmbedBuilder WithMalUpdateProviderFooter(this DiscordEmbedBuilder builder)
	{
		builder.Footer = MalUpdateFooter;
		return builder;
	}

	public static async Task<DiscordEmbedBuilder> ToDiscordEmbedBuilderAsync<TLe, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
		this TLe listEntry, User user, IMyAnimeListEnrichment enrichment, MalUser dbUser, CancellationToken cancellationToken)
		where TLe : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
		where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
		where TStatus : BaseListEntryStatus<TListStatus>
		where TMediaType : unmanaged, Enum
		where TNodeStatus : unmanaged, Enum
		where TListStatus : unmanaged, Enum
	{
		static string SubEntriesProgress(ulong progressedValue, uint totalValue, bool isInPlans, string ending) =>
			progressedValue switch
			{
				0ul when totalValue == 0u => "",
				_ when progressedValue == totalValue || (isInPlans && progressedValue == 0ul) => $"{totalValue} {ending}",
				_ when totalValue == 0u => $"{progressedValue}/? {ending}",
				_ => $"{progressedValue}/{totalValue} {ending}",
			};

		static string TitleMediaTypeString(string title, string mediaType, MalUserFeatures features) =>
			title.EndsWith(mediaType, StringComparison.OrdinalIgnoreCase) || title.EndsWith($"({mediaType})", StringComparison.OrdinalIgnoreCase) || !features.HasFlag(MalUserFeatures.MediaFormat)
				? title
				: $"{title} ({mediaType})";

		var features = dbUser.Features;
		var eb = new DiscordEmbedBuilder().WithThumbnail(listEntry.Node.Picture?.Large ?? listEntry.Node.Picture?.Medium!)
										  .WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl).WithTimestamp(listEntry.Status.UpdatedAt);
		if (listEntry.Status.Score != 0)
		{
			eb.AddFieldIfPresent("Score", listEntry.Status.Score.ToString(NumberFormatInfo.InvariantInfo), inline: true);
		}

		string userProgressText;
		switch (listEntry)
		{
			case AnimeListEntry ale:
				{
					var progress = ale.Status.Status.Humanize(LetterCasing.Sentence);
					var episodeProgress = SubEntriesProgress(ale.Status.EpisodesWatched, ale.Node.Episodes, ale.Status.Status == AnimeListStatus.PlanToWatch, "ep.");
					userProgressText = episodeProgress is [] ? progress : $"{progress} - {episodeProgress}";
					break;
				}

			case MangaListEntry mle:
				{
					var progress = mle.Status.Status.Humanize(LetterCasing.Sentence);
					var chapterProgress = SubEntriesProgress(mle.Status.ChaptersRead, mle.Node.TotalChapters, mle.Status.Status == MangaListStatus.PlanToRead, "ch. ");
					var volumeProgress =
						SubEntriesProgress(mle.Status.VolumesRead, mle.Node.TotalVolumes, mle.Status.Status == MangaListStatus.PlanToRead, "v.");

					userProgressText = string.IsNullOrWhiteSpace(volumeProgress) || !string.IsNullOrWhiteSpace(chapterProgress)
						? $"{progress} - {chapterProgress}{volumeProgress}" : progress;
					break;
				}

			default:
				{
					throw new UnreachableException("We shouldnt have any other entry type other than Anime and Manga");
				}
		}

		eb.AddFieldIfPresent("Progress", userProgressText, inline: true);

		if (listEntry.Status.ReprogressTimes > 0)
		{
			eb.AddField($"{(listEntry is AnimeListEntry ? "Rewatch" : "Reread")} times", listEntry.Status.ReprogressTimes.ToString(NumberFormatInfo.InvariantInfo));
		}

		var shortTitle = TitleMediaTypeString(listEntry.Node.Title, listEntry.Node.MediaType.Humanize(), features);
		string title;
		if (features.HasFlag(MalUserFeatures.MediaStatus))
		{
			var entryStatus = listEntry switch
			{
				AnimeListEntry animeListEntry => animeListEntry.Node.Status.Humanize(LetterCasing.Sentence),
				MangaListEntry mangaListEntry => mangaListEntry.Node.Status.Humanize(LetterCasing.Sentence),
				_ => throw new UnreachableException(),
			};
			title = $"{shortTitle} [{entryStatus}]";
		}
		else
		{
			title = shortTitle;
		}

		const int discordTitleLimit = 256;

		if (title.Length <= discordTitleLimit)
		{
			eb.Url = listEntry.Node.Url;
			eb.Title = title;
		}
		else
		{
			eb.Description = Formatter.MaskedUrl(title, new(listEntry.Node.Url));
		}

		var mediaInfo = features.HasAnyFlag(MalUserFeatures.Demographic, MalUserFeatures.Themes) ? listEntry switch
		{
			MangaListEntry => await enrichment.GetMangaDetailsAsync(listEntry.Node.Id, cancellationToken),
			AnimeListEntry => await enrichment.GetAnimeDetailsAsync(listEntry.Node.Id, cancellationToken),
			_ => throw new UnreachableException(),
		} : MediaInfo.Empty;

		if (features.HasFlag(MalUserFeatures.Tags) && listEntry.Status.Tags is not null and not [])
		{
			var joinedTags = listEntry.Status.Tags.JoinToString();
			if (!string.IsNullOrWhiteSpace(joinedTags))
			{
				AddAsFieldOrTruncateToDescription(eb, "Tags", joinedTags);
			}
		}

		if (features.HasFlag(MalUserFeatures.Comments) && !string.IsNullOrWhiteSpace(listEntry.Status.Comments))
		{
			AddAsFieldOrTruncateToDescription(eb, "Comments", listEntry.Status.Comments);
		}

		AddGenres(eb, listEntry.Node.Genres, features);
		AddThemes(eb, mediaInfo, features);
		AddDemographic(eb, mediaInfo, features);

		if (features.HasFlag(MalUserFeatures.Synopsis) && !string.IsNullOrWhiteSpace(listEntry.Node.Synopsis))
		{
			var index = listEntry.Node.Synopsis.IndexOf('\n', StringComparison.InvariantCulture);
			var text = listEntry.Node.Synopsis;
			if (index > 0)
			{
				text = text[..index];
			}

			if (!string.IsNullOrWhiteSpace(text))
			{
				AddAsFieldOrTruncateToDescription(eb, "Synopsis", text, inline: false);
			}
		}

		if (features.HasFlag(MalUserFeatures.Dates) && (listEntry.Status.StartDate is not null || listEntry.Status.FinishDate is not null))
		{
			var isStartNull = listEntry.Status.StartDate is null;
			var isFinishNull = listEntry.Status.FinishDate is null;
			var fieldTitle = (isStartNull, isFinishNull) switch
			{
				(false, false) => "Start Date - Finish Date",
				(false, true) => "Start Date",
				(true, false) => "Finish Date",
				_ => throw new UnreachableException(),
			};
			const string format = "dd/MM/yyyy";
			var value = (isStartNull, isFinishNull) switch
			{
#pragma warning disable S103
				// Split this 202 characters long line
				(false, false) => $"{listEntry.Status.StartDate!.Value.ToString(format, DateTimeFormatInfo.InvariantInfo)} - {listEntry.Status.FinishDate!.Value.ToString(format, DateTimeFormatInfo.InvariantInfo)}",
#pragma warning restore
				(false, true) => listEntry.Status.StartDate!.Value.ToString(format, DateTimeFormatInfo.InvariantInfo),
				(true, false) => listEntry.Status.FinishDate!.Value.ToString(format, DateTimeFormatInfo.InvariantInfo),
				_ => throw new UnreachableException(),
			};

			eb.AddFieldIfPresent(fieldTitle, value);
		}

		AddStudios(eb, (listEntry as AnimeListEntry)?.Node.Studios, features);

		if (features.HasFlag(MalUserFeatures.Seiyu) && listEntry is AnimeListEntry)
		{
			var seiyu = await enrichment.GetAnimeSeiyuAsync(listEntry.Node.Id, cancellationToken);
			AddSeiyu(eb, seiyu, features);
		}

		AddAuthors(eb, (listEntry as MangaListEntry)?.Node.Authors, features);

		var updateType = listEntry switch
		{
			MangaListEntry m => m.Status.Status switch
			{
				_ when m.Status.IsRereading => MalUpdateType.RereadingManga,
				MangaListStatus.OnHold => MalUpdateType.OnHoldManga,
				MangaListStatus.Reading => MalUpdateType.Reading,
				MangaListStatus.Dropped => MalUpdateType.DroppedManga,
				MangaListStatus.PlanToRead => MalUpdateType.PlanToRead,
				MangaListStatus.Completed => MalUpdateType.CompletedManga,
				_ => throw new ArgumentOutOfRangeException(nameof(listEntry), "Invalid status"),
			},

			AnimeListEntry a => a.Status.Status switch
			{
				_ when a.Status.IsRewatching => MalUpdateType.RewatchingAnime,
				AnimeListStatus.OnHold => MalUpdateType.OnHoldManga,
				AnimeListStatus.Watching => MalUpdateType.Watching,
				AnimeListStatus.Dropped => MalUpdateType.DroppedAnime,
				AnimeListStatus.PlanToWatch => MalUpdateType.PlanToWatch,
				AnimeListStatus.Completed => MalUpdateType.CompletedAnime,
				_ => throw new ArgumentOutOfRangeException(nameof(listEntry), "Invalid status"),
			},

			_ => throw new ArgumentOutOfRangeException(nameof(listEntry), "Invalid status"),
		};

		var storedColor = dbUser.Colors.Find(c => c.UpdateType == (byte)updateType);

		var color = Colors[listEntry.Status.GetStatusAsUnderlyingType()];

		if (storedColor is not null)
		{
			color = new(storedColor.ColorValue);
		}

		eb.WithColor(color);
		return eb;
	}

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
			AddThemes(builder, mediaInfo, features);
			AddDemographic(builder, mediaInfo, features);
		}

		if (isAnime && features.HasFlag(MalUserFeatures.Seiyu))
		{
			var seiyu = await enrichment.GetAnimeSeiyuAsync(result.Id, cancellationToken).ConfigureAwait(false);
			AddSeiyu(builder, seiyu, features);
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
		IReadOnlyList<Studio>? studios = null;
		IReadOnlyList<Author>? mangakas = null;
		switch (result)
		{
			case AnimeSearchResult anime:
				mediaPath = "anime";
				total = anime.Episodes == 0U ? null : $"{anime.Episodes.ToString(CultureInfo.InvariantCulture)} ep.";
				season = anime.StartSeason is { Season: not AnimeSeason.Unknown, Year: not 0U } startSeason
					? $"{startSeason.Season.Humanize(LetterCasing.Sentence)} {startSeason.Year.ToString(CultureInfo.InvariantCulture)}"
					: null;
				studios = anime.Studios;
				break;
			case MangaSearchResult manga:
				mediaPath = "manga";
				total = FormatMangaTotal(manga);
				season = null;
				mangakas = manga.Authors;
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

	internal static string? FormatMangaTotal(MangaSearchResult manga)
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
		MalUserFeatures features,
		string? mediaType,
		string? status,
		string? total,
		string? season,
		IReadOnlyList<Studio>? studios,
		IReadOnlyList<Author>? mangakas,
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

		AddStudios(builder, studios, features);
		AddAuthors(builder, mangakas, features);
		AddGenres(builder, result.Genres, features);

		return builder;
	}

	internal static void AddSynopsis(DiscordEmbedBuilder eb, string? synopsis, MalUserFeatures features)
	{
		if (!features.HasFlag(MalUserFeatures.Synopsis) || string.IsNullOrWhiteSpace(synopsis))
		{
			return;
		}

		var text = synopsis.RemoveSourceTail().Trim().Truncate(SynopsisLimit);
		if (!string.IsNullOrWhiteSpace(text))
		{
			AddAsFieldOrTruncateToDescription(eb, "Synopsis", text, inline: false);
		}
	}

	internal static void AddGenres(DiscordEmbedBuilder eb, IReadOnlyList<Genre>? genres, MalUserFeatures features)
	{
		if (!features.HasFlag(MalUserFeatures.Genres) || genres is not { Count: > 0 })
		{
			return;
		}

		var value = genres.Select(static x => x.Name).Where(static name => !string.IsNullOrWhiteSpace(name)).Take(7).Select(static name => name.ToFirstCharUpperCase())
						  .JoinToString();
		if (!string.IsNullOrWhiteSpace(value))
		{
			AddAsFieldOrTruncateToDescription(eb, "Genres", value);
		}
	}

	internal static void AddStudios(DiscordEmbedBuilder eb, IReadOnlyList<Studio>? studios, MalUserFeatures features)
	{
		if (!features.HasFlag(MalUserFeatures.Studio) || studios is not { Count: > 0 })
		{
			return;
		}

		var value = studios.Take(3).Select(static s => Formatter.MaskedUrl(s.Name, new Uri(s.Url))).JoinToString();
		if (!string.IsNullOrWhiteSpace(value))
		{
			AddAsFieldOrTruncateToDescription(eb, "Studios", value);
		}
	}

	internal static void AddAuthors(DiscordEmbedBuilder eb, IReadOnlyList<Author>? authors, MalUserFeatures features)
	{
		if (!features.HasFlag(MalUserFeatures.Mangakas) || authors is not { Count: > 0 })
		{
			return;
		}

		var value = authors.Take(7).Select(static author =>
		{
			var person = author.Person;
			var name = $"{(!string.IsNullOrWhiteSpace(person.LastName) ? $"{person.LastName}, {person.FirstName}" : person.FirstName)} ({author.Role})";

			return Formatter.MaskedUrl(name, new Uri(person.Url));
		}).JoinToString();
		if (!string.IsNullOrWhiteSpace(value))
		{
			AddAsFieldOrTruncateToDescription(eb, "Authors", value);
		}
	}

	internal static void AddSeiyu(DiscordEmbedBuilder eb, IReadOnlyList<SeyuInfo> seiyu, MalUserFeatures features)
	{
		if (!features.HasFlag(MalUserFeatures.Seiyu) || seiyu is not { Count: > 0 })
		{
			return;
		}

		var value = seiyu.Take(7).Select(static info => Formatter.MaskedUrl(info.Name, new Uri(info.Url))).JoinToString();
		if (!string.IsNullOrWhiteSpace(value))
		{
			AddAsFieldOrTruncateToDescription(eb, "Seiyu", value);
		}
	}

	internal static void AddThemes(DiscordEmbedBuilder eb, MediaInfo mediaInfo, MalUserFeatures features)
	{
		if (!features.HasFlag(MalUserFeatures.Themes) || mediaInfo.Themes is [])
		{
			return;
		}

		var value = mediaInfo.Themes.Take(7).JoinToString();
		if (!string.IsNullOrWhiteSpace(value))
		{
			AddAsFieldOrTruncateToDescription(eb, "Themes", value);
		}
	}

	internal static void AddDemographic(DiscordEmbedBuilder eb, MediaInfo mediaInfo, MalUserFeatures features)
	{
		if (!features.HasFlag(MalUserFeatures.Demographic) || mediaInfo.Demographic is [])
		{
			return;
		}

		var value = mediaInfo.Demographic.Take(3).JoinToString();
		if (!string.IsNullOrWhiteSpace(value))
		{
			AddAsFieldOrTruncateToDescription(eb, "Demographic", value);
		}
	}

	private static void AddAsFieldOrTruncateToDescription(DiscordEmbedBuilder eb, string fieldName, string fieldValue, bool inline = true)
	{
		const int nonTruncatableFieldLimit = 1024;
		if (fieldValue.Length <= nonTruncatableFieldLimit)
		{
			eb.AddField(fieldName, fieldValue, inline);
		}
		else
		{
			var l = eb.Description?.Length ?? 0;
			var descToAdd = $"{fieldName}\n{fieldValue}".Truncate(2048 - l - 1, Truncator.FixedNumberOfCharacters);

			if (string.IsNullOrWhiteSpace(eb.Description))
			{
				eb.WithDescription(descToAdd);
			}
			else
			{
				eb.Description += $"{'\n'}{descToAdd}";
			}
		}
	}

	private static bool IsValidUrl([NotNullWhen(true)] string? url)
	{
		const int urlLimit = 2048;
		return !string.IsNullOrWhiteSpace(url) && url.Length <= urlLimit;
	}
}
