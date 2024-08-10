// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.Common.Enums;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.UpdateProvider.Achievements;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal static partial class Extensions
{
	[GeneratedRegex(@"\[.+?\]", RegexOptions.Compiled | RegexOptions.NonBacktracking, 2000 /*2s*/)]
	private static partial Regex BracketsRegex();

	private static readonly DiscordEmbedBuilder.EmbedFooter ShikiUpdateProviderFooter = new()
	{
		Text = "Shikimori",
		IconUrl = Constants.IconUrl,
	};

	private static readonly (string, ProgressType)[] Progresses =
	[
		("смотрю", ProgressType.InProgress), ("пересматриваю", ProgressType.InProgress), ("запланировано", ProgressType.InPlans),
		("брошено", ProgressType.Dropped), ("просмотрены", ProgressType.InProgress), ("просмотрен", ProgressType.Completed),
		("отложено", ProgressType.OnHold), ("прочитана", ProgressType.InProgress), ("прочитаны", ProgressType.InProgress), ("прочитан", ProgressType.Completed), ("перечитываю", ProgressType.InProgress),
		("читаю", ProgressType.InProgress),
	];

	private static readonly DiscordColor[] Colors =
	[
		Constants.ShikiBlue,
		Constants.ShikiGreen,
		Constants.ShikiGrey,
		Constants.ShikiRed,
		Constants.ShikiBlue,
	];

	private static readonly FrozenSet<string> MangakaRelatedRoles = new[] { "story", "art", "creator", "design" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
	private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

	public static DiscordEmbedBuilder WithShikiAuthor(this DiscordEmbedBuilder builder, UserInfo user) =>
		builder.WithAuthor(user.Nickname, user.Url, user.ImageUrl);

	public static async Task<IReadOnlyList<History>> GetAllUserHistoryAfterEntryAsync(
																this IShikiClient client,
																uint userId,
																ulong limitHistoryEntryId,
																ShikiUserFeatures features,
																CancellationToken cancellationToken = default)
	{
		uint page = 1;
		byte limit = 10;
		var options = features switch
		{
			_ when features.HasFlag(ShikiUserFeatures.AnimeList) && features.HasFlag(ShikiUserFeatures.MangaList) => HistoryRequestOptions.Any,
			_ when features.HasFlag(ShikiUserFeatures.AnimeList) && !features.HasFlag(ShikiUserFeatures.MangaList) => HistoryRequestOptions.Anime,
			_ when features.HasFlag(ShikiUserFeatures.MangaList) && !features.HasFlag(ShikiUserFeatures.AnimeList) => HistoryRequestOptions.Manga,
			_ => ThrowHelper.ThrowArgumentOutOfRangeException<HistoryRequestOptions>(nameof(features), features, message: null),
		};

		var (data, hasNextPage) = await client.GetUserHistoryAsync(userId, page, limit, options, cancellationToken);
		var unpaginatedRes = data.Where(e => e.Id > limitHistoryEntryId).ToArray();
		if (unpaginatedRes.Length != data.Length || !hasNextPage)
		{
			return unpaginatedRes;
		}

		var acc = new List<History>(50);
		var hnp = true;
		var isLimitReached = false;
		for (page = 1, limit = 100; hnp && !isLimitReached; page++)
		{
			var (paginatedData, paginatedHasNextPage) =
				await client.GetUserHistoryAsync(userId, page, limit, options, cancellationToken);
			hnp = paginatedHasNextPage;
			var toAcc = paginatedData.Where(e => e.Id > limitHistoryEntryId).ToArray();
			isLimitReached = paginatedData.Length == toAcc.Length;
			acc.AddRange(toAcc);
		}

		return acc;
	}

	public static List<List<History>> GroupSimilarHistoryEntries(this IReadOnlyList<History> source)
	{
		var res = new List<List<History>>(5);
		var group = new List<History>(1);
		foreach (var he in source.OrderBy(x => x.Id))
		{
			if (group.Count == 0 || group.TrueForAll(hge => hge.Target?.Id == he.Target?.Id))
			{
				group.Add(he);
			}
			else
			{
				res.Add(group);
				group = [he];
			}
		}

		if (group is not [])
		{
			res.Add(group);
		}

		return res;
	}

	public static DiscordEmbedBuilder ToDiscordEmbed(this HistoryMediaRoles history, UserInfo user, ShikiUser dbUser)
	{
		static ProgressType CalculateProgressType(List<History> histories)
		{
			for (var i = histories.Count - 1; i >= 0; i--)
			{
				foreach (var (d, prog) in Progresses)
				{
					if (histories[i].Description.Contains(d, StringComparison.OrdinalIgnoreCase))
					{
						return prog;
					}
				}
			}

			return ProgressType.InProgress;
		}

		var features = dbUser.Features;

		var first = history.HistoryEntries[0];
		var eb = new DiscordEmbedBuilder().WithTimestamp(first.CreatedAt).WithShikiAuthor(user).WithColor(Constants.ShikiBlue);
		var desc = string.Join("; ", history.HistoryEntries.Select(h => h.Description)).StripHtml().ToSentenceCase(RuCulture)!;
		eb.WithDescription(desc);
		var target = history.HistoryEntries.Find(x => x.Target is not null)?.Target;
		if (target is null)
		{
			return eb;
		}

		var progress = CalculateProgressType(history.HistoryEntries);

		var updateType = progress switch
		{
			ProgressType.OnHold when target.Type is ListEntryType.Anime => ShikiUpdateType.PausedAnime,
			ProgressType.InProgress when target.Type is ListEntryType.Anime => ShikiUpdateType.Watching,
			ProgressType.Dropped when target.Type is ListEntryType.Anime => ShikiUpdateType.DroppedAnime,
			ProgressType.InPlans when target.Type is ListEntryType.Anime => ShikiUpdateType.PlanToWatch,
			ProgressType.Completed when target.Type is ListEntryType.Anime => ShikiUpdateType.CompletedAnime,

			ProgressType.OnHold when target.Type is ListEntryType.Manga => ShikiUpdateType.PausedManga,
			ProgressType.InProgress when target.Type is ListEntryType.Manga => ShikiUpdateType.Reading,
			ProgressType.Dropped when target.Type is ListEntryType.Manga => ShikiUpdateType.DroppedManga,
			ProgressType.InPlans when target.Type is ListEntryType.Manga => ShikiUpdateType.PlanToRead,
			ProgressType.Completed when target.Type is ListEntryType.Manga => ShikiUpdateType.CompletedManga,
			_ => throw new ArgumentOutOfRangeException(nameof(history), "Invalid status"),
		};

		var storedColor = dbUser.Colors.Find(c => c.UpdateType == (byte)updateType);

		var color = Colors[(int)progress];

		if (storedColor is not null)
		{
			color = new(storedColor.ColorValue);
		}

		eb = eb.WithColor(color);

		var titleSb = new StringBuilder();

		titleSb.Append(target.GetNameOrAltName(features));

		if (features.HasFlag(ShikiUserFeatures.MediaFormat))
		{
			titleSb.Append(CultureInfo.InvariantCulture, $" ({(target.Kind ?? "Unknown").Humanize(LetterCasing.Sentence)})");
		}

		if (features.HasFlag(ShikiUserFeatures.MediaStatus))
		{
			titleSb.AppendLine(CultureInfo.InvariantCulture, $" [{target.Status.Humanize(LetterCasing.Sentence)}]");
		}

		eb.WithTitle(titleSb.ToString()).WithUrl(target.Url).WithThumbnail(target.ImageUrl);

		if (target.Chapters.HasValue && target.Chapters != 0)
		{
			eb.AddField("Total", $"{target.Chapters.Value} ch. {target.Volumes.GetValueOrDefault()} v.", inline: true);
		}
		else if (target.Episodes.HasValue)
		{
			var episodes = target switch
			{
				_ when target.Episodes != 0u => target.Episodes.Value,
				_ when target.EpisodesAired != 0u => target.EpisodesAired.GetValueOrDefault(),
				_ => 0u,
			};
			if (episodes != 0)
			{
				eb.AddField("Total", $"{episodes} ep.", inline: true);
			}
		}

		eb.FillMediaInfo(history.Media, history.Roles, features, target.Type);

		return eb;
	}

	public static DiscordEmbedBuilder ToDiscordEmbed(this FavouriteMediaRoles favouriteEntry, UserInfo user, bool added, ShikiUser dbUser)
	{
		var features = dbUser.Features;
		var color = dbUser.Colors.Find(added
			? c => c.UpdateType == (byte)ShikiUpdateType.FavoriteAdded
			: c => c.UpdateType == (byte)ShikiUpdateType.FavoriteRemoved)?.ColorValue ?? (added ? Constants.ShikiGreen : Constants.ShikiRed);

		var favouriteName = favouriteEntry.FavouriteEntry.GetNameOrAltName(features);
		var eb = new DiscordEmbedBuilder
			{
				Url = favouriteEntry.FavouriteEntry.Url,
				Title =
					$"{favouriteName} [{(favouriteEntry.FavouriteEntry.SpecificType ?? favouriteEntry.FavouriteEntry.GenericType)?.ToFirstCharUpperCase()}]",
			}.WithThumbnail(favouriteEntry.FavouriteEntry.ImageUrl).WithDescription($"{(added ? "Added" : "Removed")} favourite")
			 .WithShikiAuthor(user)
			 .WithColor(color);

		var isAnime = favouriteEntry.FavouriteEntry.GenericType!.Contains("anime", StringComparison.OrdinalIgnoreCase);
		var isManga = favouriteEntry.FavouriteEntry.GenericType!.Contains("manga", StringComparison.OrdinalIgnoreCase);
		if ((isAnime || isManga) && (favouriteEntry.Media is not null || favouriteEntry.Roles is not null))
		{
			eb.FillMediaInfo(favouriteEntry.Media, favouriteEntry.Roles, features, isAnime ? ListEntryType.Anime : ListEntryType.Manga);
		}

		return eb;
	}

	public static DiscordEmbedBuilder ToDiscordEmbed(this ShikiAchievement achievement, UserInfo user, ShikiUserFeatures features)
	{
		const string baseUrl = $"{Wrapper.Abstractions.Constants.BaseUrl}/achievements/";
		var eb = new DiscordEmbedBuilder
		{
			Title = features.HasFlag(ShikiUserFeatures.Russian) ? achievement.TitleRussian : achievement.TitleEnglish,
			Description = (features.HasFlag(ShikiUserFeatures.Russian) ? achievement.TextRussian : achievement.TextEnglish) ?? "",
			Color = achievement.BorderColor,
			Url = baseUrl + achievement.Id,
		}.WithThumbnail(achievement.Image).WithShikiAuthor(user);
		if (achievement.HumanName is not null)
		{
			eb.AddField(achievement.HumanName, $"Level {achievement.Level:D}");
		}

		return eb;
	}

	public static DiscordEmbedBuilder WithShikiUpdateProviderFooter(this DiscordEmbedBuilder eb)
	{
		eb.Footer = ShikiUpdateProviderFooter;
		return eb;
	}

	public static string GetNameOrAltName(this IMultiLanguageName namedEntity, ShikiUserFeatures features) =>
		GetNameOrAltName(namedEntity, features.HasFlag(ShikiUserFeatures.Russian));

	public static string GetNameOrAltName(this IMultiLanguageName namedEntity, bool useRussianAsMain) => useRussianAsMain switch
	{
		true => string.IsNullOrWhiteSpace(namedEntity.RussianName) ? namedEntity.Name! : namedEntity.RussianName,
		_ => string.IsNullOrWhiteSpace(namedEntity.Name) ? namedEntity.RussianName! : namedEntity.Name,
	};

	private static void FillMediaInfo(this DiscordEmbedBuilder eb, BaseMedia? media, IReadOnlyList<Role>? roles, ShikiUserFeatures features, ListEntryType type)
	{
		if (type == ListEntryType.Anime)
		{
			if (features.HasFlag(ShikiUserFeatures.Studio) && media is AnimeMedia anime)
			{
				var text = string.Join(", ", anime.Studios.Select(x => Formatter.MaskedUrl(x.Name, new(x.Url))));
				if (!string.IsNullOrEmpty(text))
				{
					eb.AddField("Studio", text, inline: true);
				}
			}

			if (features.HasFlag(ShikiUserFeatures.Director) && roles is not null and not [])
			{
				var role = roles.FirstOrDefault(
					x => x.Person is not null && x.Name.Any(y => y.Equals("Director", StringComparison.OrdinalIgnoreCase)));
				if (role is not null)
				{
					eb.AddField("Director", role.Person!.GetNameOrAltName(features), inline: true);
				}
			}
		}
		else
		{
			if (features.HasFlag(ShikiUserFeatures.Publisher) && media is MangaMedia manga)
			{
				var text = string.Join(", ", manga.Publishers.Select(x => Formatter.MaskedUrl(x.Name, new(x.Url))));
				if (!string.IsNullOrEmpty(text))
				{
					eb.AddField("Publisher", text, inline: true);
				}
			}

			if (features.HasFlag(ShikiUserFeatures.Mangaka) && roles is not null and not [])
			{
				var mangakas = string.Join(
					", ",
					roles.Where(x =>
						x.Person is not null && MangakaRelatedRoles.Overlaps(x.Name)).Take(5).Select(x =>
					{
						var nameOfRole = features.HasFlag(ShikiUserFeatures.Russian)
							? x.RussianName.FirstOrDefault(y => !string.IsNullOrWhiteSpace(y)) ?? x.Name[0]
							: x.Name.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? x.RussianName[0];
						return $"{Formatter.MaskedUrl(x.Person!.GetNameOrAltName(features), new(x.Person!.Url))} - {nameOfRole}";
					}));
				if (!string.IsNullOrEmpty(mangakas))
				{
					eb.AddField("Author", mangakas, inline: true);
				}
			}
		}

		if (features.HasFlag(ShikiUserFeatures.Description) && !string.IsNullOrWhiteSpace(media?.Description))
		{
			var text = BracketsRegex().Replace(media.Description, "").Truncate(350);
			if (!string.IsNullOrEmpty(text))
			{
				eb.AddField("Description", text);
			}
		}

		if (features.HasFlag(ShikiUserFeatures.Genres))
		{
			var text = string.Join(", ", media!.Genres.Take(7).Select(x => x.GetNameOrAltName(features)));
			if (!string.IsNullOrEmpty(text))
			{
				eb.AddField("Genres", text);
			}
		}
	}

	public static FavoriteIdType[] ToFavoriteIdType<T>(this T favorites)
		where T : IReadOnlyCollection<FavouriteEntry>
	{
		return [..favorites.Select(x => new FavoriteIdType(x.Id, (byte)x.GenericType![0])).OrderBy(x => x.Id).ThenBy(x => x.Type)];
	}
}