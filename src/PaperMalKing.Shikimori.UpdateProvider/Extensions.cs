// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.Common.Enums;
using PaperMalKing.Database.Models.Shikimori;
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
		IconUrl = Constants.ICON_URL
	};

	private static readonly FrozenDictionary<ProgressType, DiscordColor> Colors = new Dictionary<ProgressType, DiscordColor>()
	{
		{ ProgressType.Completed, Constants.ShikiGreen },
		{ ProgressType.Dropped, Constants.ShikiRed },
		{ ProgressType.InPlans, Constants.ShikiBlue },
		{ ProgressType.InProgress, Constants.ShikiBlue },
		{ ProgressType.OnHold, Constants.ShikiGrey }
	}.ToFrozenDictionary(true);

	private static readonly (string, ProgressType)[] Progresses = new[]
	{
		("смотрю", ProgressType.InProgress), ("пересматриваю", ProgressType.InProgress), ("запланировано", ProgressType.InPlans),
		("брошено", ProgressType.Dropped), ("просмотрены", ProgressType.InProgress), ("просмотрен", ProgressType.Completed),
		("отложено", ProgressType.OnHold), ("прочитан", ProgressType.Completed), ("перечитываю", ProgressType.InProgress),
		("читаю", ProgressType.InProgress)
	};

	private static readonly string[] mangakaRelatedRoles = { "story", "art", "creator", "design" };
	private static CultureInfo ruCulture = CultureInfo.GetCultureInfo("ru-RU");

	public static DiscordEmbedBuilder WithShikiAuthor(this DiscordEmbedBuilder builder, UserInfo user) =>
		builder.WithAuthor(user.Nickname, user.Url, user.ImageUrl);

	public static async Task<IReadOnlyList<History>> GetAllUserHistoryAfterEntryAsync(this IShikiClient client, uint userId,
																					  ulong limitHistoryEntryId, ShikiUserFeatures features,
																					  CancellationToken cancellationToken = default)
	{
		uint page = 1;
		byte limit = 10;
		var options = features switch
		{
			_ when features.HasFlag(ShikiUserFeatures.AnimeList) && features.HasFlag(ShikiUserFeatures.MangaList) => HistoryRequestOptions.Any,
			_ when features.HasFlag(ShikiUserFeatures.AnimeList) && !features.HasFlag(ShikiUserFeatures.MangaList) => HistoryRequestOptions.Anime,
			_ when features.HasFlag(ShikiUserFeatures.MangaList) && !features.HasFlag(ShikiUserFeatures.AnimeList) => HistoryRequestOptions.Manga,
			_ => throw new ArgumentOutOfRangeException(nameof(features), features, null)
		};

		var (data, hasNextPage) = await client.GetUserHistoryAsync(userId, page, limit, options, cancellationToken).ConfigureAwait(false);
		var unpaginatedRes = data.Where(e => e.Id > limitHistoryEntryId).ToArray();
		if (unpaginatedRes.Length != data.Length || !hasNextPage)
			return unpaginatedRes;

		var acc = new List<History>(50);
		var hnp = true;
		var isLimitReached = false;
		for (page = 1, limit = 100; hnp && !isLimitReached; page++)
		{
			var (paginatedData, paginatedHasNextPage) =
				await client.GetUserHistoryAsync(userId, page, limit, options, cancellationToken).ConfigureAwait(false);
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
				group = new(1)
				{
					he
				};
			}
		}

		if (group.Count > 0)
			res.Add(group);
		return res;
	}

	public static DiscordEmbedBuilder ToDiscordEmbed(this HistoryMediaRoles history, UserInfo user, ShikiUserFeatures features)
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

		var first = history.HistoryEntries[0];
		var eb = new DiscordEmbedBuilder().WithTimestamp(first.CreatedAt).WithShikiAuthor(user).WithColor(Constants.ShikiBlue);
		var desc = string.Join("; ", history.HistoryEntries.Select(h => h.Description)).StripHtml().ToSentenceCase(ruCulture)!;
		eb.WithDescription(desc).WithColor(Colors[CalculateProgressType(history.HistoryEntries)]);
		var target = history.HistoryEntries.Find(x => x.Target is not null)?.Target;
		if (target is null)
			return eb;

		var titleSb = new StringBuilder();

		titleSb.Append(target.GetNameOrAltName(features));

		if (features.HasFlag(ShikiUserFeatures.MediaFormat))
		{
			titleSb.Append($" ({(target.Kind ?? "Unknown").Humanize(LetterCasing.Sentence)})");
		}

		if (features.HasFlag(ShikiUserFeatures.MediaStatus))
		{
			titleSb.AppendLine($" [{target.Status.Humanize(LetterCasing.Sentence)}]");
		}

		eb.WithTitle(titleSb.ToString()).WithUrl(target.Url).WithThumbnail(target.ImageUrl);

		if (target.Chapters.HasValue && target.Chapters != 0)
		{
			eb.AddField("Total", $"{target.Chapters.Value} ch. {target.Volumes.GetValueOrDefault()} v.", true);
		}
		else if (target.Episodes.HasValue)
		{
			var episodes = target switch
			{
				_ when target.Episodes != 0u      => target.Episodes.Value,
				_ when target.EpisodesAired != 0u => target.EpisodesAired.GetValueOrDefault(),
				_                                 => 0u
			};
			if (episodes != 0)
				eb.AddField("Total", $"{episodes} ep.", true);
		}

		eb.FillMediaInfo(history.Media, history.Roles, features, target.Type);

		return eb;
	}

	public static DiscordEmbedBuilder ToDiscordEmbed(this FavouriteMediaRoles favouriteEntry, UserInfo user, bool added, ShikiUserFeatures features)
	{
		var favouriteName = favouriteEntry.FavouriteEntry.GetNameOrAltName(features);
		var eb = new DiscordEmbedBuilder
			{
				Url = favouriteEntry.FavouriteEntry.Url,
				Title =
					$"{favouriteName} [{(favouriteEntry.FavouriteEntry.SpecificType ?? favouriteEntry.FavouriteEntry.GenericType)?.ToFirstCharUpperCase()}]"
			}.WithThumbnail(favouriteEntry.FavouriteEntry.ImageUrl).WithDescription($"{(added ? "Added" : "Removed")} favourite")
			 .WithShikiAuthor(user)
			 .WithColor(added ? Constants.ShikiGreen : Constants.ShikiRed);

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
		return new DiscordEmbedBuilder()
		{
			Title = features.HasFlag(ShikiUserFeatures.Russian) ? achievement.TitleRussian : achievement.TitleEnglish,
			Description = features.HasFlag(ShikiUserFeatures.Russian) ? achievement.TextRussian : achievement.TextEnglish,
			Color = achievement.BorderColor
		}.WithThumbnail(achievement.Image).WithShikiAuthor(user);
	}

	public static DiscordEmbedBuilder WithShikiUpdateProviderFooter(this DiscordEmbedBuilder eb)
	{
		eb.Footer = ShikiUpdateProviderFooter;
		return eb;
	}

	public static string GetNameOrAltName(this IMultiLanguageName namedEntity, ShikiUserFeatures features) =>
		GetNameOrAltName(namedEntity, features.HasFlag(ShikiUserFeatures.Russian));

	public static string GetNameOrAltName(this IMultiLanguageName namedEntity, bool useRussianAsMain) => (useRussianAsMain switch
	{
		true => string.IsNullOrWhiteSpace(namedEntity.RussianName) ? namedEntity.Name : namedEntity.RussianName,
		_    => string.IsNullOrWhiteSpace(namedEntity.Name) ? namedEntity.RussianName : namedEntity.Name,
	})!;

	private static void FillMediaInfo(this DiscordEmbedBuilder eb, BaseMedia? media, IReadOnlyList<Role>? roles, ShikiUserFeatures features,
									  ListEntryType type)
	{
		if (type == ListEntryType.Anime)
		{
			if (features.HasFlag(ShikiUserFeatures.Studio) && media is AnimeMedia anime)
			{
				var text = string.Join(", ", anime.Studios.Select(x => Formatter.MaskedUrl(x.Name, new(x.Url))));
				if (!string.IsNullOrEmpty(text))
				{
					eb.AddField("Studio", text, true);
				}
			}

			if (features.HasFlag(ShikiUserFeatures.Director) && roles!.Count > 0)
			{
				var role = roles.FirstOrDefault(
					x => x.Person is not null && x.Name.Any(y => y.Equals("Director", StringComparison.OrdinalIgnoreCase)));
				if (role is not null)
				{
					eb.AddField("Director", role.Person!.GetNameOrAltName(features), true);
				}
			}
		}
		else
		{
			if (features.HasFlag(ShikiUserFeatures.Publisher) && media is MangaMedia manga)
			{
				var text = string.Join(", ", manga.Publishers.Select(x => Formatter.MaskedUrl(x.Name, new(x.Url))));
				if (string.IsNullOrEmpty(text))
				{
					eb.AddField("Publisher", text, true);
				}
			}

			if (features.HasFlag(ShikiUserFeatures.Mangaka) && roles!.Count > 0)
			{
				var mangakas = string.Join(", ",
					roles!.Where(x =>
						x.Person is not null && mangakaRelatedRoles.Exists(y => x.Name.Any(z => z.Contains(y, StringComparison.OrdinalIgnoreCase)))).Take(5).Select(x =>
					{
						var nameOfRole = features.HasFlag(ShikiUserFeatures.Russian)
							? x.RussianName.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? x.Name[0]
							: x.Name.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? x.RussianName[0];
						return $"{Formatter.MaskedUrl(x.Person!.GetNameOrAltName(features), new(x.Person!.Url))} - {nameOfRole}";
					}));
				if (!string.IsNullOrEmpty(mangakas))
				{
					eb.AddField("Author", mangakas, true);
				}
			}
		}

		if (features.HasFlag(ShikiUserFeatures.Description))
		{
			var text = BracketsRegex().Replace(media!.Description, "").Truncate(350);
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
}