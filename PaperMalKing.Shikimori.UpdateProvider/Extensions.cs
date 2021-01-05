using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.Common.Enums;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Models;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	internal static class Extensions
	{
		private static readonly ReverseMarkdown.Converter HtmlToMarkdownConverter = new();

		private static readonly DiscordEmbedBuilder.EmbedFooter ShikiUpdateProviderFooter = new()
		{
			Text = "Shikimori",
			IconUrl = Constants.ICON_URL
		};

		private static readonly Dictionary<ProgressType, DiscordColor> Colors = new()
		{
			{ProgressType.Completed, Constants.ShikiGreen},
			{ProgressType.Dropped, Constants.ShikiRed},
			{ProgressType.InPlans, Constants.ShikiBlue},
			{ProgressType.InProgress, Constants.ShikiBlue},
			{ProgressType.OnHold, Constants.ShikiGrey}
		};

		public static DiscordEmbedBuilder WithShikiAuthor(this DiscordEmbedBuilder builder, UserInfo user) =>
			builder.WithAuthor(user.Nickname, user.Url, user.ImageUrl);

		public static async Task<List<History>> GetAllUserHistoryAfterEntryAsync(this ShikiClient client, ulong userId, ulong limitHistoryEntryId,
																				 CancellationToken cancellationToken = default)
		{
			uint page = 1;
			byte limit = 10;
			var (data, hasNextPage) = await client.GetUserHistoryAsync(userId, page, limit, cancellationToken);
			var unpaginatedRes = data.Where(e => e.Id > limitHistoryEntryId).ToList();
			if (unpaginatedRes.Count != data.Length || !hasNextPage)
				return unpaginatedRes;

			var acc = new List<History>(50);
			var hnp = true;
			var isLimitReached = false;
			for (page = 1, limit = 100; hnp && !isLimitReached; page++)
			{
				var (paginatedData, paginatedHasNextPage) = await client.GetUserHistoryAsync(userId, page, limit, cancellationToken);
				hnp = paginatedHasNextPage;
				var toAcc = paginatedData.Where(e => e.Id > limitHistoryEntryId).ToArray();
				isLimitReached = paginatedData.Length == toAcc.Length;
				acc.AddRange(toAcc);
			}

			return acc;
		}

		public static DiscordEmbedBuilder ToDiscordEmbed(this Favourites.FavouriteEntry favouriteEntry, UserInfo user, bool added)
		{
			return new DiscordEmbedBuilder
				{
					Url = favouriteEntry.Url,
					Title = $"{favouriteEntry.Name} [{(favouriteEntry.GenericType ?? favouriteEntry.SpecificType).Humanize(LetterCasing.Sentence)}]"
				}.WithThumbnail(favouriteEntry.ImageUrl).AddField("Status", $"{(added ? "Added" : "removed")} favourite", true).WithShikiAuthor(user)
				 .WithColor(added ? Constants.ShikiGreen : Constants.ShikiRed);
		}

		public static List<List<History>> GroupSimilarHistoryEntries(this List<History> source)
		{
			source.Sort((h1, h2) => Comparer<ulong>.Default.Compare(h1.Id, h2.Id));
			var res = new List<List<History>>(5);
			var group = new List<History>(1);
			foreach (var he in source)
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

		public static DiscordEmbedBuilder ToDiscordEmbed(this List<History> history, UserInfo user)
		{
			var first = history[0];
			var ruCulture = CultureInfo.GetCultureInfo("ru-RU");
			var eb = new DiscordEmbedBuilder().WithTimestamp(first.CreatedAt).WithShikiAuthor(user).WithColor(Constants.ShikiBlue);
			var desc = HtmlToMarkdownConverter.Convert(string.Join("; ", history.Select(h => h.Description))).ToSentenceCase(ruCulture)!;
			eb.WithDescription(desc);

			var dict = new Dictionary<int, ProgressType>();
			dict.TryAdd(desc.LastIndexOf("смотрю", StringComparison.OrdinalIgnoreCase), ProgressType.InProgress);
			dict.TryAdd(desc.LastIndexOf("пересматриваю", StringComparison.OrdinalIgnoreCase), ProgressType.InProgress);
			dict.TryAdd(desc.LastIndexOf("запланировано", StringComparison.OrdinalIgnoreCase), ProgressType.InPlans);
			dict.TryAdd(desc.LastIndexOf("брошено", StringComparison.OrdinalIgnoreCase), ProgressType.Dropped);
			dict.TryAdd(desc.LastIndexOf("просмотрен", StringComparison.OrdinalIgnoreCase), ProgressType.Completed);
			dict.TryAdd(desc.LastIndexOf("отложено", StringComparison.OrdinalIgnoreCase), ProgressType.OnHold);

			dict.TryAdd(desc.LastIndexOf("прочитан", StringComparison.OrdinalIgnoreCase), ProgressType.Completed);
			dict.TryAdd(desc.LastIndexOf("перечитываю", StringComparison.OrdinalIgnoreCase), ProgressType.InProgress);
			var progress = dict.OrderByDescending(kvp => kvp.Key).First().Value;

			eb.WithColor(Colors[progress]);
			var firstTarget = first?.Target;
			if (firstTarget == null)
				return eb;

			eb.WithTitle(
				  $"{firstTarget.Name} ({firstTarget.Kind.Humanize(LetterCasing.Sentence)}) [{firstTarget.Status.Humanize(LetterCasing.Sentence)}]")
			  .WithUrl(firstTarget.Url).WithThumbnail(firstTarget.ImageUrl);

			if (firstTarget.Chapters.HasValue && firstTarget.Chapters != 0)
			{
				eb.AddField("Total", $"{"ch".ToQuantity(firstTarget.Chapters.Value)}, {"v".ToQuantity(firstTarget.Volumes!.Value)}", true);
			}
			else if (firstTarget.Episodes.HasValue)
			{
				var episodes = firstTarget switch
				{
					var i when firstTarget.Episodes != 0      => firstTarget.Episodes.Value,
					var i when firstTarget.EpisodesAired != 0 => firstTarget.EpisodesAired!.Value,
					_                                         => 0
				};
				if (episodes != 0)
					eb.AddField("Total", "ep".ToQuantity(episodes), true);
			}

			return eb;
		}

		public static DiscordEmbedBuilder WithShikiUpdateProviderFooter(this DiscordEmbedBuilder eb)
		{
			eb.Footer = ShikiUpdateProviderFooter;
			return eb;
		}
	}
}