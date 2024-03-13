﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal static class Extensions
{
	private static readonly DiscordEmbedBuilder.EmbedFooter MalUpdateFooter = new()
	{
		IconUrl = Constants.FavIcon,
		Text = Constants.Name,
	};

	private static readonly DiscordColor[] Colors =
	[
		DiscordColor.NotQuiteBlack, Constants.MalGreen, Constants.MalBlue, Constants.MalYellow, Constants.MalRed, Constants.MalGrey,
	];

	public static ParserOptions ToParserOptions(this MalUserFeatures features)
	{
		var options = ParserOptions.None;
		if (features.HasFlag(MalUserFeatures.AnimeList))
		{
			options |= ParserOptions.AnimeList;
		}

		if (features.HasFlag(MalUserFeatures.MangaList))
		{
			options |= ParserOptions.MangaList;
		}

		if (features.HasFlag(MalUserFeatures.Favorites))
		{
			options |= ParserOptions.Favorites;
		}

		return options;
	}

	public static TRequestOptions ToRequestOptions<TRequestOptions>(this MalUserFeatures features)
		where TRequestOptions : unmanaged, Enum
	{
		Debug.Assert(typeof(TRequestOptions) == typeof(AnimeFieldsToRequest) || typeof(TRequestOptions) == typeof(MangaFieldsToRequest),
			$"Only {nameof(AnimeFieldsToRequest)} and {nameof(MangaFieldsToRequest)} are supported");
		AnimeFieldsToRequest fields = default;
		if (features.HasFlag(MalUserFeatures.Synopsis))
		{
			fields |= AnimeFieldsToRequest.Synopsis;
		}

		if (features.HasFlag(MalUserFeatures.Genres))
		{
			fields |= AnimeFieldsToRequest.Genres;
		}

		if (features.HasFlag(MalUserFeatures.Tags))
		{
			fields |= AnimeFieldsToRequest.Tags;
		}

		if (features.HasFlag(MalUserFeatures.Comments))
		{
			fields |= AnimeFieldsToRequest.Comments;
		}

		if (typeof(TRequestOptions) == typeof(MangaFieldsToRequest) && features.HasFlag(MalUserFeatures.Mangakas))
		{
			var mangaFields = Unsafe.As<AnimeFieldsToRequest, MangaFieldsToRequest>(ref fields);
			mangaFields |= MangaFieldsToRequest.Authors;
			return Unsafe.As<MangaFieldsToRequest, TRequestOptions>(ref mangaFields);
		}

		if (features.HasFlag(MalUserFeatures.Studio))
		{
			fields |= AnimeFieldsToRequest.Studio;
		}

		return Unsafe.As<AnimeFieldsToRequest, TRequestOptions>(ref fields);
	}

	public static T ToDbFavorite<T>(this BaseFavorite baseFavorite, MalUser user)
		where T : BaseMalFavorite
	{
		return baseFavorite switch
		{
			FavoriteAnime favoriteAnime => favoriteAnime.ToMalFavoriteAnime(user) as T,
			FavoriteCharacter favoriteCharacter => favoriteCharacter.ToMalFavoriteCharacter(user) as T,
			FavoriteManga favoriteManga => favoriteManga.ToMalFavoriteManga(user) as T,
			FavoritePerson favoritePerson => favoritePerson.ToMalFavoritePerson(user) as T,
			FavoriteCompany favoriteCompany => favoriteCompany.ToMalFavoriteCompany(user) as T,
			_ => ThrowHelper.ThrowInvalidOperationException<T>(),
		}

		?? ThrowHelper.ThrowInvalidOperationException<T>();
	}

	public static MalFavoriteAnime ToMalFavoriteAnime(this FavoriteAnime anime, MalUser user) => new()
	{
		Id = anime.Url.Id,
		Name = anime.Name,
		Type = anime.Type,
		ImageUrl = anime.ImageUrl,
		NameUrl = anime.Url.Url,
		StartYear = anime.StartYear,
		User = user,
		UserId = user.UserId,
	};

	public static MalFavoriteManga ToMalFavoriteManga(this FavoriteManga manga, MalUser user) => new()
	{
		Id = manga.Url.Id,
		Name = manga.Name,
		Type = manga.Type,
		ImageUrl = manga.ImageUrl,
		NameUrl = manga.Url.Url,
		StartYear = manga.StartYear,
		User = user,
		UserId = user.UserId,
	};

	public static MalFavoriteCompany ToMalFavoriteCompany(this FavoriteCompany company, MalUser user) => new()
	{
		Id = company.Url.Id,
		Name = company.Name,
		ImageUrl = company.ImageUrl,
		NameUrl = company.Url.Url,
		User = user,
		UserId = user.UserId,
	};

	public static MalFavoriteCharacter ToMalFavoriteCharacter(this FavoriteCharacter character, MalUser user) => new()
	{
		Id = character.Url.Id,
		Name = character.Name,
		ImageUrl = character.ImageUrl,
		NameUrl = character.Url.Url,
		FromTitleName = character.FromName,
		User = user,
		UserId = user.UserId,
	};

	public static MalFavoritePerson ToMalFavoritePerson(this FavoritePerson person, MalUser user) => new()
	{
		Id = person.Url.Id,
		Name = person.Name,
		ImageUrl = person.ImageUrl,
		NameUrl = person.Url.Url,
		User = user,
		UserId = user.UserId,
	};

	public static DiscordEmbedBuilder WithMalUpdateProviderFooter(this DiscordEmbedBuilder builder)
	{
		builder.Footer = MalUpdateFooter;
		return builder;
	}

	public static DiscordEmbedBuilder ToDiscordEmbedBuilder(this BaseMalFavorite favorite, bool added)
	{
		var eb = new DiscordEmbedBuilder
		{
			Url = favorite.NameUrl,
		}.WithThumbnail(favorite.ImageUrl!).WithDescription($"{(added ? "Added" : "Removed")} favorite");

		eb.WithColor(added ? Constants.MalGreen : Constants.MalRed);

		var title = favorite switch
		{
			BaseMalListFavorite baseListFavorite => $"{baseListFavorite.Name} ({baseListFavorite.Type}) [{baseListFavorite.StartYear}]",
			_ => favorite.Name,
		};
		eb.WithTitle(title);

		if (favorite is MalFavoriteCharacter favoriteCharacter)
		{
			eb.AddField("From", favoriteCharacter.FromTitleName, inline: true);
		}

		return eb;
	}

	public static async Task<DiscordEmbedBuilder> ToDiscordEmbedBuilderAsync<TLe, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
		this TLe listEntry, User user, MalUserFeatures features, IMyAnimeListClient client, CancellationToken cancellationToken)
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

		var eb = new DiscordEmbedBuilder().WithThumbnail(listEntry.Node.Picture?.Large ?? listEntry.Node.Picture?.Medium!)
										  .WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl).WithTimestamp(listEntry.Status.UpdatedAt);
		if (listEntry.Status.Score != 0)
		{
			eb.AddField("Score", listEntry.Status.Score.ToString(NumberFormatInfo.InvariantInfo), inline: true);
		}

		string userProgressText;
		switch (listEntry)
		{
			case AnimeListEntry ale:
				{
					var progress = ale.Status.Status.Humanize(LetterCasing.Sentence);
					var episodeProgress = SubEntriesProgress(ale.Status.EpisodesWatched, ale.Node.Episodes, ale.Status.Status == AnimeListStatus.plan_to_watch, "ep.");
					userProgressText = episodeProgress is not [] ? $"{progress} - {episodeProgress}" : progress;
					break;
				}

			case MangaListEntry mle:
				{
					var progress = mle.Status.Status.Humanize(LetterCasing.Sentence)!;
					var chapterProgress = SubEntriesProgress(mle.Status.ChaptersRead, mle.Node.TotalChapters, mle.Status.Status == MangaListStatus.plan_to_read, "ch. ");
					var volumeProgress =
						SubEntriesProgress(mle.Status.VolumesRead, mle.Node.TotalVolumes, mle.Status.Status == MangaListStatus.plan_to_read, "v.");
					userProgressText = string.IsNullOrEmpty(volumeProgress) || !string.IsNullOrEmpty(chapterProgress)
						? $"{progress} - {chapterProgress}{volumeProgress}" : progress;
					break;
				}

			default:
				{
					throw new UnreachableException("We shouldnt have any other entry type other than Anime and Manga");
				}
		}

		eb.AddField("Progress", userProgressText, inline: true);

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

		if (title.Length <= 256)
		{
			eb.Url = listEntry.Node.Url;
			eb.Title = title;
		}
		else
		{
			eb.Description = Formatter.MaskedUrl(title, new Uri(listEntry.Node.Url));
		}

		var mediaInfo = features.HasFlag(MalUserFeatures.Demographic) || features.HasFlag(MalUserFeatures.Themes) ? listEntry switch
		{
			MangaListEntry => await client.GetMangaDetailsAsync(listEntry.Node.Id, cancellationToken),
			AnimeListEntry => await client.GetAnimeDetailsAsync(listEntry.Node.Id, cancellationToken),
			_ => throw new UnreachableException(),
		} : MediaInfo.Empty;

		if (features.HasFlag(MalUserFeatures.Tags) && listEntry.Status.Tags is not null and not [])
		{
			var joinedTags = string.Join(", ", listEntry.Status.Tags);
			if (!string.IsNullOrWhiteSpace(joinedTags))
			{
				AddAsFieldOrTruncateToDescription(eb, "Tags", joinedTags);
			}
		}

		if (features.HasFlag(MalUserFeatures.Comments) && !string.IsNullOrWhiteSpace(listEntry.Status.Comments))
		{
			AddAsFieldOrTruncateToDescription(eb, "Comments", listEntry.Status.Comments);
		}

		if (features.HasFlag(MalUserFeatures.Genres) && listEntry.Node.Genres is not null and not [])
		{
			var genres = string.Join(", ", listEntry.Node.Genres.Take(7).Select(x => x.Name.ToFirstCharUpperCase()));
			if (!string.IsNullOrWhiteSpace(genres))
			{
				AddAsFieldOrTruncateToDescription(eb, "Genres", genres);
			}
		}

		if (features.HasFlag(MalUserFeatures.Themes) && mediaInfo.Themes is not [])
		{
			var themes = string.Join(", ", mediaInfo.Themes.Take(7));
			if (!string.IsNullOrWhiteSpace(themes))
			{
				AddAsFieldOrTruncateToDescription(eb, "Themes", themes);
			}
		}

		if (features.HasFlag(MalUserFeatures.Demographic) && mediaInfo.Demographic is not [])
		{
			var demographic = string.Join(", ", mediaInfo.Demographic.Take(3));
			if (!string.IsNullOrWhiteSpace(demographic))
			{
				AddAsFieldOrTruncateToDescription(eb, "Demographic", demographic);
			}
		}

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
				(false, false) => $"{listEntry.Status.StartDate!.Value.ToString(format, DateTimeFormatInfo.InvariantInfo)} - {listEntry.Status.FinishDate!.Value.ToString(format, DateTimeFormatInfo.InvariantInfo)}",
				(false, true) => listEntry.Status.StartDate!.Value.ToString(format, DateTimeFormatInfo.InvariantInfo),
				(true, false) => listEntry.Status.FinishDate!.Value.ToString(format, DateTimeFormatInfo.InvariantInfo),
				_ => throw new UnreachableException(),
			};
			eb.AddField(fieldTitle, value);
		}

		if (features.HasFlag(MalUserFeatures.Studio) && listEntry is AnimeListEntry { Node.Studios: not null and not [] } aListEntry)
		{
			var studios = string.Join(", ", aListEntry.Node.Studios.Select(x => Formatter.MaskedUrl(x.Name, new(x.Url))));
			if (!string.IsNullOrWhiteSpace(studios))
			{
				AddAsFieldOrTruncateToDescription(eb, "Studios", studios);
			}
		}

		if (features.HasFlag(MalUserFeatures.Seiyu) && listEntry is AnimeListEntry)
		{
			var seiyu = await client.GetAnimeSeiyuAsync(listEntry.Node.Id, cancellationToken);
			var text = string.Join(", ", seiyu.Take(7).Select(x => Formatter.MaskedUrl(x.Name, new(x.Url))));
			if (!string.IsNullOrWhiteSpace(text))
			{
				AddAsFieldOrTruncateToDescription(eb, "Seiyu", text);
			}
		}

		if (features.HasFlag(MalUserFeatures.Mangakas) && listEntry is MangaListEntry { Node.Authors: not null and not [] } mListEntry)
		{
			var authors = string.Join(", ", mListEntry.Node.Authors.Take(7).Select(x =>
			{
				var name =
					$"{(!string.IsNullOrEmpty(x.Person.LastName) ? $"{x.Person.LastName}, {x.Person.FirstName}" : x.Person.FirstName)} ({x.Role})";
				return Formatter.MaskedUrl(name, new Uri(x.Person.Url));
			}));

			if (!string.IsNullOrWhiteSpace(authors))
			{
				AddAsFieldOrTruncateToDescription(eb, "Authors", authors);
			}
		}

		eb.WithColor(Colors[listEntry.Status.GetStatusAsUnderlyingType()]);
		return eb;
	}

	private static void AddAsFieldOrTruncateToDescription(DiscordEmbedBuilder eb, string fieldName, string fieldValue, bool inline = true)
	{
		if (fieldValue.Length <= 1024)
		{
			eb.AddField(fieldName, fieldValue, inline);
		}
		else
		{
			var l = eb.Description?.Length ?? 0;
			var descToAdd = $"{fieldName}\n{fieldValue}".Truncate(2048 - l - 1, Truncator.FixedNumberOfCharacters);
			if (string.IsNullOrEmpty(eb.Description))
			{
				eb.WithDescription(descToAdd);
			}
			else
			{
				eb.Description += $"{'\n'}{descToAdd}";
			}
		}
	}

	public static Span<FavoriteIdType> GetFavoriteIdTypesFromFavorites(this UserFavorites favorites)
	{
		static void Add(List<FavoriteIdType> aggregator, IReadOnlyList<BaseFavorite> favs, MalFavoriteType type)
		{
			aggregator.AddRange(favs.Select(x => new FavoriteIdType(x.Url.Id, (byte)type)));
		}

		var result = new List<FavoriteIdType>(favorites.Count);
		Add(result, favorites.FavoriteAnime, MalFavoriteType.Anime);
		Add(result, favorites.FavoriteManga, MalFavoriteType.Manga);
		Add(result, favorites.FavoriteCharacters, MalFavoriteType.Character);
		Add(result, favorites.FavoritePeople, MalFavoriteType.Person);
		Add(result, favorites.FavoriteCompanies, MalFavoriteType.Company);
		result.Sort();
		return CollectionsMarshal.AsSpan(result);
	}
}