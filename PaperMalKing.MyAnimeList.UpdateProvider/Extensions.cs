// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.MangaList;
using WConstants = PaperMalKing.MyAnimeList.Wrapper.Constants;


namespace PaperMalKing.UpdatesProviders.MyAnimeList;

internal static class Extensions
{
	private static readonly DiscordEmbedBuilder.EmbedFooter MalUpdateFooter = new()
	{
		IconUrl = WConstants.FAV_ICON,
		Text = Constants.Name
	};

	private static readonly Dictionary<byte, DiscordColor> Colors = new()
	{
		{(byte)AnimeListStatus.watching, Constants.MalGreen},
		{(byte)AnimeListStatus.completed, Constants.MalBlue},
		{(byte)AnimeListStatus.on_hold, Constants.MalYellow},
		{(byte)AnimeListStatus.dropped, Constants.MalRed},
		{(byte)AnimeListStatus.plan_to_watch, Constants.MalGrey},
	};

	internal static ParserOptions ToParserOptions(this MalUserFeatures features)
	{
		var options = ParserOptions.None;
		if (features.HasFlag(MalUserFeatures.AnimeList)) options |= ParserOptions.AnimeList;
		if (features.HasFlag(MalUserFeatures.MangaList)) options |= ParserOptions.MangaList;
		if (features.HasFlag(MalUserFeatures.Favorites)) options |= ParserOptions.Favorites;

		return options;
	}

	internal static TRequestOptions ToRequestOptions<TRequestOptions>(this MalUserFeatures features) where TRequestOptions : unmanaged, Enum
	{
		Debug.Assert(typeof(TRequestOptions) == typeof(AnimeFieldsToRequest) || typeof(TRequestOptions) == typeof(MangaFieldsToRequest));
		AnimeFieldsToRequest fields = default;
		if ((features & MalUserFeatures.Synopsis) != 0)
		{
			fields |= AnimeFieldsToRequest.Synopsis;
		}

		if ((features & MalUserFeatures.Genres) != 0)
		{
			fields |= AnimeFieldsToRequest.Genres;
		}

		if ((features & MalUserFeatures.Tags) != 0)
		{
			fields |= AnimeFieldsToRequest.Tags;
		}

		if ((features & MalUserFeatures.Comments) != 0)
		{
			fields |= AnimeFieldsToRequest.Comments;
		}

		if (typeof(TRequestOptions) == typeof(MangaFieldsToRequest) && (features & MalUserFeatures.Mangakas) != 0)
		{
			var mangaFields = Unsafe.As<AnimeFieldsToRequest, MangaFieldsToRequest>(ref fields);
			mangaFields |= MangaFieldsToRequest.Authors;
			return Unsafe.As<MangaFieldsToRequest, TRequestOptions>(ref mangaFields);
		}
		if((features & MalUserFeatures.Studio) != 0)
		{
			fields |= AnimeFieldsToRequest.Studio;
		}

		return Unsafe.As<AnimeFieldsToRequest, TRequestOptions>(ref fields);
	}

	internal static T ToDbFavorite<T>(this BaseFavorite baseFavorite, MalUser user) where T : class, IMalFavorite
	{
		return baseFavorite switch
		{
			FavoriteAnime favoriteAnime => favoriteAnime.ToMalFavoriteAnime(user) as T,
			FavoriteCharacter favoriteCharacter => favoriteCharacter.ToMalFavoriteCharacter(user) as T,
			FavoriteManga favoriteManga => favoriteManga.ToMalFavoriteManga(user) as T,
			FavoritePerson favoritePerson => favoritePerson.ToMalFavoritePerson(user) as T,
			FavoriteCompany favoriteCompany => favoriteCompany.ToMalFavoriteCompany(user) as T,
			_ => throw new InvalidOperationException()
		} ?? throw new InvalidOperationException();
	}

	internal static MalFavoriteAnime ToMalFavoriteAnime(this FavoriteAnime anime, MalUser user) => new()
	{
		Id = anime.Url.Id,
		Name = anime.Name,
		Type = anime.Type,
		ImageUrl = anime.ImageUrl,
		NameUrl = anime.Url.Url,
		StartYear = anime.StartYear,
		User = user,
		UserId = user.UserId
	};

	internal static MalFavoriteManga ToMalFavoriteManga(this FavoriteManga manga, MalUser user) => new()
	{
		Id = manga.Url.Id,
		Name = manga.Name,
		Type = manga.Type,
		ImageUrl = manga.ImageUrl,
		NameUrl = manga.Url.Url,
		StartYear = manga.StartYear,
		User = user,
		UserId = user.UserId
	};

	internal static MalFavoriteCompany ToMalFavoriteCompany(this FavoriteCompany company, MalUser user) => new()
	{
		Id = company.Url.Id,
		Name = company.Name,
		ImageUrl = company.ImageUrl,
		NameUrl = company.Url.Url,
		User = user,
		UserId = user.UserId
	};

	internal static MalFavoriteCharacter ToMalFavoriteCharacter(this FavoriteCharacter character, MalUser user) => new()
	{
		Id = character.Url.Id,
		Name = character.Name,
		ImageUrl = character.ImageUrl,
		NameUrl = character.Url.Url,
		FromTitleName = character.FromName,
		User = user,
		UserId = user.UserId
	};

	internal static MalFavoritePerson ToMalFavoritePerson(this FavoritePerson person, MalUser user) => new()
	{
		Id = person.Url.Id,
		Name = person.Name,
		ImageUrl = person.ImageUrl,
		NameUrl = person.Url.Url,
		User = user,
		UserId = user.UserId
	};

	internal static DiscordEmbedBuilder WithMalUpdateProviderFooter(this DiscordEmbedBuilder builder)
	{
		builder.Footer = MalUpdateFooter;
		return builder;
	}

	internal static DiscordEmbedBuilder ToDiscordEmbedBuilder(this IMalFavorite favorite, bool added)
	{
		var eb = new DiscordEmbedBuilder
		{
			Url = favorite.NameUrl
		}.WithThumbnail(favorite.ImageUrl).WithDescription($"{(added ? "Added" : "Removed")} favorite");

		eb.WithColor(added ? Constants.MalGreen : Constants.MalRed);

		var title = favorite switch
		{
			IMalListFavorite baseListFavorite => $"{baseListFavorite.Name} ({baseListFavorite.Type}) [{baseListFavorite.StartYear}]",
			_ => favorite.Name
		};
		eb.WithTitle(title);

		if (favorite is MalFavoriteCharacter favoriteCharacter)
			eb.AddField("From", favoriteCharacter.FromTitleName, true);

		return eb;
	}

	internal static DiscordEmbedBuilder ToDiscordEmbedBuilder<TLe, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(this TLe listEntry, User user, MalUserFeatures features) where TLe : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
																						where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
																						where TStatus : BaseListEntryStatus<TListStatus>
																						where TMediaType : unmanaged, Enum
																						where TNodeStatus : unmanaged, Enum
																						where TListStatus : unmanaged, Enum
	{
		static string SubEntriesProgress(ulong progressedValue, uint totalValue, bool isInPlans, string ending) =>
			progressedValue switch
			{
				0 when totalValue == 0 => string.Empty,
				_ when progressedValue == totalValue || (isInPlans && progressedValue == 0) => $"{totalValue} {ending}",
				_ when totalValue == 0 => $"{progressedValue}/? {ending}",
				_ => $"{progressedValue}/{totalValue} {ending}"
			};

		static string TitleMediaTypeString(string title, string mediaType, MalUserFeatures features) =>
			title.EndsWith(mediaType, StringComparison.OrdinalIgnoreCase) || title.EndsWith($"({mediaType})", StringComparison.OrdinalIgnoreCase) || !features.HasFlag(MalUserFeatures.MediaFormat)
				? title
				: $"{title} ({mediaType})";

		var eb = new DiscordEmbedBuilder().WithThumbnail(listEntry.Node.Picture?.Large ?? listEntry.Node.Picture?.Medium)
										  .WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl).WithTimestamp(listEntry.Status.UpdatedAt);


		if (listEntry.Status.Score != 0)
			eb.AddField("Score", listEntry.Status.Score.ToString(), true);

		string userProgressText;
		switch (listEntry)
		{
			case AnimeListEntry ale:
				{
					var progress = ale.Status.Status.Humanize(LetterCasing.Sentence);
					string episodeProgress = SubEntriesProgress(ale.Status.EpisodesWatched, ale.Node.Episodes,
						ale.Status.Status == AnimeListStatus.plan_to_watch, "ep.");
					userProgressText = episodeProgress.Length != 0 ? $"{progress} - {episodeProgress}" : progress;
					break;
				}
			case MangaListEntry mle:
				{
					string progress = mle.Status.Status.Humanize(LetterCasing.Sentence)!;
					string chapterProgress = SubEntriesProgress(mle.Status.ChaptersRead, mle.Node.TotalChapters,
						mle.Status.Status == MangaListStatus.plan_to_read, "ch. ");
					string volumeProgress =
						SubEntriesProgress(mle.Status.ReadVolumes, mle.Node.TotalVolumes, mle.Status.Status == MangaListStatus.plan_to_read, "v.");
					userProgressText = chapterProgress.Length != 0 ||
#pragma warning disable CA1508
									   volumeProgress.Length != 0
#pragma warning restore CA1508
						? $"{progress} - {chapterProgress}{volumeProgress}" : progress;
					break;
				}
			default:
			{
				throw new UnreachableException("We shouldnt have any other entry type other than Anime and Manga");
			}
		}

		eb.AddField("Progress", userProgressText, true);

		var shortTitle = TitleMediaTypeString(listEntry.Node.Title, listEntry.Node.MediaType.Humanize(), features);
		string title;
		if (features.HasFlag(MalUserFeatures.MediaStatus))
		{
			var entryStatus = listEntry switch
			{
				AnimeListEntry animeListEntry => animeListEntry.Node.Status.Humanize(LetterCasing.Sentence),
				MangaListEntry mangaListEntry => mangaListEntry.Node.Status.Humanize(LetterCasing.Sentence),
				_                             => throw new UnreachableException()
			};
			title = $"{shortTitle} [{entryStatus}]";
		}
		else
			title = shortTitle;

		if (title.Length <= 256)
		{
			eb.Url = listEntry.Node.Url;
			eb.Title = title;
		}
		else
			eb.Description = Formatter.MaskedUrl(title, new Uri(listEntry.Node.Url));

		if ((features & MalUserFeatures.Tags) != 0 && listEntry.Status.Tags?.Count is not null and not 0)
		{
			var joinedTags = string.Join(", ", listEntry.Status.Tags);
			AddAsFieldOrTruncateToDescription(eb, "Tags", joinedTags);
		}

		if ((features & MalUserFeatures.Comments) != 0 && !string.IsNullOrWhiteSpace(listEntry.Status.Comments))
		{
			AddAsFieldOrTruncateToDescription(eb, "Comments", listEntry.Status.Comments);
		}

		if ((features & MalUserFeatures.Genres) != 0 && listEntry.Node.Genres?.Count is not null and not 0)
		{
			var genres = string.Join(", ", listEntry.Node.Genres.Take(7).Select(x => x.Name.Humanize(LetterCasing.Title)));
			AddAsFieldOrTruncateToDescription(eb, "Genres", genres);
		}

		if ((features & MalUserFeatures.Synopsis) != 0 && !string.IsNullOrWhiteSpace(listEntry.Node.Synopsis))
		{
			var shortSynopsis = listEntry.Node.Synopsis.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			AddAsFieldOrTruncateToDescription(eb, "Synopsis", shortSynopsis[0], false);
		}

		if ((features & MalUserFeatures.Dates) != 0 && (listEntry.Status.StartDate is not null || listEntry.Status.FinishDate is not null))
		{
			var isStartNull = listEntry.Status.StartDate is null;
			var isFinishNull = listEntry.Status.FinishDate is null;
			var fieldTitle = (isStartNull, isFinishNull) switch
			{
				(false, false) => "Start Date - Finish Date",
				(false, true)  => "Start Date",
				(true, false)  => "Finish Date",
				_              => throw new UnreachableException()
			};
			var format = "dd/MM/yyyy";
			var value = (isStartNull, isFinishNull) switch
			{
				(false, false) => $"{listEntry.Status.StartDate!.Value.ToString(format)} - {listEntry.Status.FinishDate!.Value.ToString(format)}",
				(false, true)  => listEntry.Status.StartDate!.Value.ToString(format),
				(true, false)  => listEntry.Status.FinishDate!.Value.ToString(format),
				_              => throw new UnreachableException()
			};
			eb.AddField(fieldTitle, value, false);
		}

		if ((features & MalUserFeatures.Studio) != 0 && listEntry is AnimeListEntry aListEntry &&
		    aListEntry.Node.Studios?.Count is not null and not 0)
		{
			var studios = string.Join(", ", aListEntry.Node.Studios.Select(x => Formatter.MaskedUrl(x.Name, new(x.Url))));
			AddAsFieldOrTruncateToDescription(eb, "Studio".ToQuantity(aListEntry.Node.Studios.Count, ShowQuantityAs.None), studios);
		}

		if ((features & MalUserFeatures.Mangakas) != 0 && listEntry is MangaListEntry mListEntry &&
		    mListEntry.Node.Authors?.Count is not null and not 0)
		{
			var authors = string.Join(", ", mListEntry.Node.Authors.Take(7).Select(x =>
			{
				var name =
					$"{(x.Person.LastName?.Length is not null and not 0 ? $"{x.Person.LastName}, {x.Person.FirstName}" : x.Person.FirstName)} ({x.Role})";
				return Formatter.MaskedUrl(name, new Uri(x.Person.Url));
			}));
			AddAsFieldOrTruncateToDescription(eb, "Authors", authors);
		}

		eb.WithColor(Colors[listEntry.Status.GetStatusAsUnderlyingType()]);
		return eb;
	}

	private static DiscordEmbedBuilder AddAsFieldOrTruncateToDescription(DiscordEmbedBuilder eb, string fieldName, string fieldValue, bool inline = true)
	{
		if (fieldValue.Length <= 1024)
			eb.AddField(fieldName, fieldValue, inline);
		else
		{
			var l = eb.Description?.Length ?? 0;
			var descToAdd = $"{fieldName}\n{fieldValue}".Truncate(2048 - l - 1, Truncator.FixedNumberOfCharacters);
			if (string.IsNullOrEmpty(eb.Description))
				eb.WithDescription(descToAdd);
			else
				eb.Description += $"{'\n'}descToAdd";
		}

		return eb;
	}

	internal static Span<FavoriteIdType> GetFavoriteIdTypesFromFavorites(this UserFavorites favorites)
	{
		static void Add(List<FavoriteIdType> aggregator, IReadOnlyList<BaseFavorite> favs, FavoriteType type)
		{
			aggregator.AddRange(favs.Select(x=>new FavoriteIdType(x.Url!.Id, (byte)type)));
		}
		var result = new List<FavoriteIdType>(favorites.Count);
		Add(result,favorites.FavoriteAnime, FavoriteType.Anime);
		Add(result,favorites.FavoriteManga, FavoriteType.Manga);
		Add(result,favorites.FavoriteCharacters, FavoriteType.Character);
		Add(result,favorites.FavoritePeople, FavoriteType.Person);
		Add(result,favorites.FavoriteCompanies, FavoriteType.Company);
		return CollectionsMarshal.AsSpan(result);
	}

	internal static List<T> AddRangeF<T>(this List<T> list, IQueryable<T> second)
	{
		list.AddRange(second.ToArray());
		return list;
	}
}