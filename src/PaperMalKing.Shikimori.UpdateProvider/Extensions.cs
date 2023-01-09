// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.Common.Enums;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Models;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal static class Extensions
{
	private static readonly DiscordEmbedBuilder.EmbedFooter ShikiUpdateProviderFooter = new()
	{
		Text = "Shikimori",
		IconUrl = Constants.ICON_URL
	};

	private static readonly Dictionary<ProgressType, DiscordColor> Colors = new()
	{
		{ ProgressType.Completed, Constants.ShikiGreen },
		{ ProgressType.Dropped, Constants.ShikiRed },
		{ ProgressType.InPlans, Constants.ShikiBlue },
		{ ProgressType.InProgress, Constants.ShikiBlue },
		{ ProgressType.OnHold, Constants.ShikiGrey }
	};

	private static readonly (string, ProgressType)[] Progresses = new[]
	{
		("смотрю", ProgressType.InProgress), ("пересматриваю", ProgressType.InProgress), ("запланировано", ProgressType.InPlans),
		("брошено", ProgressType.Dropped), ("просмотрены", ProgressType.InProgress), ("просмотрен", ProgressType.Completed),
		("отложено", ProgressType.OnHold), ("прочитан", ProgressType.Completed), ("перечитываю", ProgressType.InProgress),
		("читаю", ProgressType.InProgress)
	};

	public static DiscordEmbedBuilder WithShikiAuthor(this DiscordEmbedBuilder builder, UserInfo user) =>
		builder.WithAuthor(user.Nickname, user.Url, user.ImageUrl);

	public static async Task<IReadOnlyList<History>> GetAllUserHistoryAfterEntryAsync(this ShikiClient client, uint userId, ulong limitHistoryEntryId,
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
		foreach (var he in source.OrderBy(x=>x.Id))
		{
			if (!group.Any() || group.TrueForAll(hge => hge.Target?.Id == he.Target?.Id))
				group.Add(he);
			else
			{
				res.Add(group);
				group = new(1)
				{
					he
				};
			}
		}

		if (group.Any())
			res.Add(group);
		return res;
	}

	public static DiscordEmbedBuilder ToDiscordEmbed(this List<History> history, UserInfo user, ShikiUserFeatures features)
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

		var first = history[0];
		var ruCulture = CultureInfo.GetCultureInfo("ru-RU");
		var eb = new DiscordEmbedBuilder().WithTimestamp(first.CreatedAt).WithShikiAuthor(user).WithColor(Constants.ShikiBlue);
		var desc = string.Join("; ", history.Select(h => h.Description)).StripHtml().ToSentenceCase(ruCulture)!;
		eb.WithDescription(desc);


		eb.WithColor(Colors[CalculateProgressType(history)]);
		var target = history.Find(x => x.Target is not null)?.Target;
		if (target is null)
			return eb;

		var titleSb = new StringBuilder();
		if ((features & ShikiUserFeatures.Russian) != 0)
			titleSb.Append(target.RussianName ?? target.Name);
		else
			titleSb.Append(target.Name);
		if ((features & ShikiUserFeatures.MediaFormat) != 0)
			titleSb.Append($" ({(target.Kind ?? "Unknown").Humanize(LetterCasing.Sentence)})");
		if ((features & ShikiUserFeatures.MediaStatus) != 0)
			titleSb.AppendLine($" [{target.Status.Humanize(LetterCasing.Sentence)}]");
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
				_                                      => 0u
			};
			if (episodes != 0)
				eb.AddField("Total", $"{episodes} ep.", true);
		}

		return eb;
	}

	public static DiscordEmbedBuilder ToDiscordEmbed(this FavouriteEntry favouriteEntry, UserInfo user, bool added, ShikiUserFeatures features)
	{
		var favouriteName = (features & ShikiUserFeatures.Russian) != 0 ? favouriteEntry.RussianName ?? favouriteEntry.Name : favouriteEntry.Name;
		return new DiscordEmbedBuilder
			{
				Url = favouriteEntry.Url,
				Title = $"{favouriteName} [{(favouriteEntry.GenericType ?? favouriteEntry.SpecificType)?.ToFirstCharUpperCase()}]"
			}.WithThumbnail(favouriteEntry.ImageUrl).WithDescription($"{(added ? "Added" : "Removed")} favourite").WithShikiAuthor(user)
			 .WithColor(added ? Constants.ShikiGreen : Constants.ShikiRed);
	}

	public static DiscordEmbedBuilder WithShikiUpdateProviderFooter(this DiscordEmbedBuilder eb)
	{
		eb.Footer = ShikiUpdateProviderFooter;
		return eb;
	}
}