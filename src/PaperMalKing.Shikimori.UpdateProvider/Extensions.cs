// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.Common.Enums;
using PaperMalKing.Common.Exceptions;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.UpdateProvider.Achievements;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Shikimori.UpdateProvider;

[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don\'t access instance data should be static", Justification = "False positive")]
internal static partial class Extensions
{
	[GeneratedRegex(@"\[.+?\]", RegexOptions.Compiled | RegexOptions.NonBacktracking, matchTimeoutMilliseconds: 1000 /*1s*/)]
	private static partial Regex BracketsRegex { get; }

	private const int DescriptionLimit = 350;

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

	private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

	extension(DiscordEmbedBuilder builder)
	{
		public DiscordEmbedBuilder WithShikiAuthor(UserInfo user) => builder.WithAuthor(user.Nickname, user.Url, user.ImageUrl);

		public DiscordEmbedBuilder WithShikiUpdateProviderFooter()
		{
			builder.Footer = ShikiUpdateProviderFooter;
			return builder;
		}

		public DiscordEmbedBuilder WithShikiMediaTitle(string name, string? kind, string? status, ShikiUserFeatures features)
		{
			var titleSb = new StringBuilder();
			titleSb.Append(name);
			if (features.HasFlag(ShikiUserFeatures.MediaFormat))
			{
				titleSb.Append(CultureInfo.InvariantCulture, $" ({(kind ?? "Unknown").Humanize(LetterCasing.Sentence)})");
			}

			if (features.HasFlag(ShikiUserFeatures.MediaStatus) && !string.IsNullOrWhiteSpace(status))
			{
				titleSb.Append(CultureInfo.InvariantCulture, $" [{status.Humanize(LetterCasing.Sentence)}]");
			}

			return builder.WithTitle(titleSb.ToString());
		}

		public DiscordEmbedBuilder WithThumbnailIfPresent(string? url)
		{
			if (!string.IsNullOrWhiteSpace(url) && string.IsNullOrWhiteSpace(builder.Thumbnail?.Url))
			{
				builder.WithThumbnail(url);
			}

			return builder;
		}

		public DiscordEmbedBuilder AddDescription(string? description, ShikiUserFeatures features)
		{
			if (!features.HasFlag(ShikiUserFeatures.Description) || string.IsNullOrWhiteSpace(description))
			{
				return builder;
			}

			return builder.AddFieldIfPresent("Description", NormalizeDescription(description));
		}

		public DiscordEmbedBuilder AddBestKnownWork(RelatedMedia? work, ShikiUserFeatures features)
		{
			if (work is null || string.IsNullOrWhiteSpace(work.Url))
			{
				return builder;
			}

			return builder.AddFieldIfPresent("From", Formatter.MaskedUrl(work.GetNameOrAltName(features), new(work.Url)), inline: true);
		}

		public DiscordEmbedBuilder FillMediaFavourite(BaseMedia media, FavouriteEntry entry, ShikiUserFeatures features)
		{
			builder.WithUrl(media.Url ?? entry.Url).WithShikiMediaTitle(FavouriteName(media, entry, features), media.Kind, media.Status, features);
			builder.AddFieldIfPresent("Total", TotalOf(media), inline: true);
			if (media.Score is > 0f)
			{
				builder.AddField("Score", media.Score.Value.ToString("0.##", CultureInfo.InvariantCulture), inline: true);
			}

			builder.FillMediaInfo(media, features, media is AnimeMedia ? ListEntryType.Anime : ListEntryType.Manga);
			return builder;
		}

		public void FillMediaInfo(BaseMedia? media, ShikiUserFeatures features, ListEntryType type)
		{
			if (media is null)
			{
				return;
			}

			if (type == ListEntryType.Anime)
			{
				if (features.HasFlag(ShikiUserFeatures.Studio) && media is AnimeMedia anime)
				{
					var text = anime.Studios.Select(x => Formatter.MaskedUrl(x.Name, new(x.Url))).JoinToString();

					builder.AddFieldIfPresent("Studio", text, inline: true);
				}

				if (features.HasFlag(ShikiUserFeatures.Director) && media.PersonRoles is not null and not [])
				{
					var role = media.PersonRoles.FirstOrDefault(x => x.Person is not null && x.Name.Any(y => y.Trim().Equals("Director", StringComparison.OrdinalIgnoreCase)));

					if (role is not null)
					{
						builder.AddField("Director", $"{Formatter.MaskedUrl(role.Person!.GetNameOrAltName(features), new(role.Person!.Url))}", inline: true);
					}
				}
			}
			else
			{
				if (features.HasFlag(ShikiUserFeatures.Publisher) && media is MangaMedia manga)
				{
					var text = manga.Publishers.Select(x => Formatter.MaskedUrl(x.Name, new(x.Url))).JoinToString();
					builder.AddFieldIfPresent("Publisher", text, inline: true);
				}

				if (features.HasFlag(ShikiUserFeatures.Mangaka) && media.PersonRoles is not null and not [])
				{
					var mangakas = media.PersonRoles.Where(x => x.Person?.IsMangaka == true).Take(5).Select(x =>
					{
						var nameOfRole = features.HasFlag(ShikiUserFeatures.Russian)
							? x.RussianName.FirstOrDefault(y => !string.IsNullOrWhiteSpace(y)) ?? x.Name[0]
							: x.Name.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? x.RussianName[0];

						return $"{Formatter.MaskedUrl(x.Person!.GetNameOrAltName(features), new(x.Person!.Url))} - {nameOfRole}";
					}).JoinToString();

					builder.AddFieldIfPresent("Author", mangakas, inline: true);
				}
			}

			builder.AddDescription(media.Description, features);

			if (features.HasFlag(ShikiUserFeatures.Genres))
			{
				var text = media.Genres.Take(7).Select(x => x.GetNameOrAltName(features)).JoinToString();
				builder.AddFieldIfPresent("Genres", text);
			}

			if (builder.Thumbnail is null || string.IsNullOrWhiteSpace(builder.Thumbnail.Url))
			{
				builder = builder.WithThumbnail(media.Poster?.BestImageUrl);
			}
		}
	}

	extension(IMultiLanguageName namedEntity)
	{
		public string GetNameOrAltName(ShikiUserFeatures features) =>
			GetNameOrAltName(namedEntity, features.HasFlag(ShikiUserFeatures.Russian));

		public string GetNameOrAltName(bool useRussianAsMain)
		{
			var (mainLang, secondaryLang) = useRussianAsMain ? (namedEntity.RussianName, namedEntity.Name) : (namedEntity.Name, namedEntity.RussianName);

			return string.IsNullOrWhiteSpace(mainLang) ? secondaryLang! : mainLang;
		}
	}

	public static async Task<IReadOnlyList<History>> GetAllUserHistoryAfterEntryAsync(this IShikiClient client, uint userId,
																					  ulong limitHistoryEntryId, ShikiUserFeatures features,
																					  CancellationToken cancellationToken)
	{
		uint page = 1;
		byte limit = 10;
		var options = (features.HasFlag(ShikiUserFeatures.AnimeList), features.HasFlag(ShikiUserFeatures.MangaList)) switch
		{
			(true, true) => HistoryRequestOptions.Any,
			(true, false) => HistoryRequestOptions.Anime,
			(false, true) => HistoryRequestOptions.Manga,
			_ => ArgumentOutOfRangeException.Throw<HistoryRequestOptions>(nameof(features), features, message: null),
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
		const int shikiMaxHistoryLimit = 100;
		for (page = 1, limit = shikiMaxHistoryLimit; hnp && !isLimitReached; page++)
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

	public static DiscordEmbedBuilder ToDiscordEmbed(this HistoryMedia history, UserInfo user, ShikiUser dbUser)
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
		var desc = history.HistoryEntries.Select(h => h.Description).JoinToString("; ").StripHtml().ToSentenceCase(RuCulture);
		eb.WithDescription(desc);
		var target = history.HistoryEntries.Find(x => x.Target is not null)?.Target;
		if (target is null)
		{
			return eb;
		}

		var progress = CalculateProgressType(history.HistoryEntries);

		var updateType = (target.Type, progress) switch
		{
			(ListEntryType.Anime, ProgressType.OnHold) => ShikiUpdateType.PausedAnime,
			(ListEntryType.Anime, ProgressType.InProgress) => ShikiUpdateType.Watching,
			(ListEntryType.Anime, ProgressType.Dropped) => ShikiUpdateType.DroppedAnime,
			(ListEntryType.Anime, ProgressType.InPlans) => ShikiUpdateType.PlanToWatch,
			(ListEntryType.Anime, ProgressType.Completed) => ShikiUpdateType.CompletedAnime,

			(ListEntryType.Manga, ProgressType.OnHold) => ShikiUpdateType.PausedManga,
			(ListEntryType.Manga, ProgressType.InProgress) => ShikiUpdateType.Reading,
			(ListEntryType.Manga, ProgressType.Dropped) => ShikiUpdateType.DroppedManga,
			(ListEntryType.Manga, ProgressType.InPlans) => ShikiUpdateType.PlanToRead,
			(ListEntryType.Manga, ProgressType.Completed) => ShikiUpdateType.CompletedManga,
			_ => throw new ArgumentOutOfRangeException(nameof(history), "Invalid status"),
		};

		var storedColor = dbUser.Colors.Find(c => c.UpdateType == (byte)updateType);

		var color = Colors[(int)progress];

		if (storedColor is not null)
		{
			color = new(storedColor.ColorValue);
		}

		eb = eb.WithColor(color);

		eb.WithShikiMediaTitle(target.GetNameOrAltName(features), target.Kind, target.Status, features).WithUrl(target.Url);

		if (!string.IsNullOrWhiteSpace(target.ImageUrl))
		{
			eb.WithThumbnail(target.ImageUrl);
		}

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
		else
		{
			// No other type besides episodes or chapters exist
		}

		eb.FillMediaInfo(history.Media, features, target.Type);

		return eb;
	}

	public static DiscordEmbedBuilder ToDiscordEmbed(this EnrichedFavourite favourite, UserInfo user, bool added, ShikiUser dbUser)
	{
		var features = dbUser.Features;
		var color = dbUser.Colors.Find(added
			? static c => c.UpdateType == (byte)ShikiUpdateType.FavoriteAdded
			: static c => c.UpdateType == (byte)ShikiUpdateType.FavoriteRemoved)?.ColorValue ?? (added ? Constants.ShikiGreen : Constants.ShikiRed);

		var entry = favourite.FavouriteEntry;
		var eb = new DiscordEmbedBuilder().WithDescription($"{(added ? "Added" : "Removed")} favourite").WithShikiAuthor(user).WithColor(color);

		if (favourite.Media is { } media)
		{
			eb.FillMediaFavourite(media, entry, features);
		}
		else if (favourite.Character is { } character)
		{
			eb.WithUrl(character.Url ?? entry.Url).WithTitle($"{FavouriteName(character, entry, features)} [Character]")
			  .AddDescription(character.Description, features).AddBestKnownWork(favourite.BestKnownWork, features)
			  .WithThumbnailIfPresent(character.Poster?.BestImageUrl);
		}
		else if (favourite.Person is { } person)
		{
			eb.WithUrl(person.Url ?? entry.Url).WithTitle($"{FavouriteName(person, entry, features)} [{person.SubKind()}]")
			  .AddBestKnownWork(favourite.BestKnownWork, features).WithThumbnailIfPresent(person.Poster?.BestImageUrl);
		}
		else
		{
			eb.WithUrl(entry.Url).WithTitle($"{entry.GetNameOrAltName(features)} [{(entry.SpecificType ?? entry.GenericType)?.ToFirstCharUpperCase()}]");
		}

		return eb.WithThumbnailIfPresent(entry.ImageUrl);
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

	private static string NormalizeDescription(string description) => BracketsRegex.Replace(description, "").Truncate(DescriptionLimit);

	private static string FavouriteName(IMultiLanguageName enriched, FavouriteEntry entry, ShikiUserFeatures features)
	{
		var name = enriched.GetNameOrAltName(features);
		return string.IsNullOrWhiteSpace(name) ? entry.GetNameOrAltName(features) : name;
	}

	private static string? TotalOf(BaseMedia media)
	{
		if (media is MangaMedia manga)
		{
			var chapters = manga.Chapters.GetValueOrDefault();
			var volumes = manga.Volumes.GetValueOrDefault();
			if (chapters > 0)
			{
				return $"{chapters} ch. {volumes} v.";
			}

			return volumes > 0 ? $"{volumes} v." : null;
		}

		if (media is not AnimeMedia anime)
		{
			return null;
		}

		var episodes = anime.Episodes is > 0 ? anime.Episodes.GetValueOrDefault() : anime.EpisodesAired.GetValueOrDefault();
		return episodes == 0 ? null : $"{episodes} ep.";
	}

	public static string SubKind(this FavouritePerson person) => person switch
	{
		{ IsMangaka: true } => "Mangaka",
		{ IsProducer: true } => "Producer",
		{ IsSeyu: true } => "Seyu",
		_ => "Person",
	};

	public static bool PrefersRolesOverWorks(this FavouritePerson person) => person is { IsSeyu: true, IsMangaka: false };

	public static RelatedMedia? BestKnownWork(this CharacterDetails details) =>
		details.Animes.Concat(details.Mangas).Where(static x => !string.IsNullOrWhiteSpace(x.Url)).MaxBy(static x => x.Score.GetValueOrDefault());

	public static RelatedMedia? BestKnownWork(this PersonDetails details, bool isSeyu) =>
		isSeyu
			? details.Roles.Select(static r => r.Media).OfType<RelatedMedia>().Where(static x => !string.IsNullOrWhiteSpace(x.Url))
					 .MaxBy(static x => x.Score.GetValueOrDefault())
			: details.Works.Select(static w => w.Media).OfType<RelatedMedia>().FirstOrDefault(static x => !string.IsNullOrWhiteSpace(x.Url));

	public static FavoriteIdType[] ToFavoriteIdType<T>(this T favorites)
		where T : IReadOnlyCollection<FavouriteEntry>
	{
		return [.. favorites.Select(static x => new FavoriteIdType(x.Id, (byte)x.GenericType![0])).Order()];
	}

	public static async Task<UpdateContents> CreateUpdateFromEmbedAsync(DiscordEmbedBuilder embedBuilder, IShikiClient shikiClient, CancellationToken cancellationToken)
	{
		List<UpdateFile> updateFiles = [];

		if (!string.IsNullOrWhiteSpace(embedBuilder.Author?.IconUrl))
		{
			var file = await shikiClient.GetImageContentAsync(embedBuilder.Author.IconUrl, cancellationToken);
			if (file is not null and not [])
			{
				const string filename = $"author.{UserInfo.ImageFormat}";
				updateFiles.Add(new()
				{
					Filename = filename,
					Content = file,
				});

				embedBuilder.Author.IconUrl = Formatter.AttachedImageUrl(filename);
			}
		}

		if (!string.IsNullOrWhiteSpace(embedBuilder.Thumbnail?.Url))
		{
			var file = await shikiClient.GetImageContentAsync(embedBuilder.Thumbnail.Url, cancellationToken);
			if (file is not null and not [])
			{
				const string filename = $"thumbnail.{HistoryTarget.ImageFormat}";
				updateFiles.Add(new()
				{
					Filename = filename,
					Content = file,
				});

				embedBuilder.Thumbnail.Url = Formatter.AttachedImageUrl(filename);
			}
		}

		return new()
		{
			EmbedBuilder = embedBuilder,
			Files = updateFiles is not [] ? [.. updateFiles] : [],
		};
	}
}