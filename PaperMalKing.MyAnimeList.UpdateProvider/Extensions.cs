using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;
using PaperMalKing.MyAnimeList.Wrapper.Models.List;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;
using WConstants = PaperMalKing.MyAnimeList.Wrapper.Constants;


namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	internal static class Extensions
	{
		private static readonly DiscordEmbedBuilder.EmbedFooter MalUpdateFooter = new()
		{
			IconUrl = WConstants.FAV_ICON,
			Text = Constants.Name
		};
		
		private static readonly Dictionary<GenericProgress, DiscordColor> Colors = new()
		{
			{GenericProgress.CurrentlyInProgress, Constants.MalGreen},
			{GenericProgress.Completed, Constants.MalBlue},
			{GenericProgress.OnHold, Constants.MalYellow},
			{GenericProgress.Dropped, Constants.MalRed},
			{GenericProgress.InPlans, Constants.MalGrey},
			{GenericProgress.Reprogressing, Constants.MalGreen},
			{GenericProgress.All, Constants.MalBlack},
			{GenericProgress.Unknown, Constants.MalBlack}
		};
		
		internal static T ToDbFavorite<T>(this BaseFavorite baseFavorite, MalUser user) where T : class, IMalFavorite
		{
			return baseFavorite switch
			{
				FavoriteAnime favoriteAnime         => favoriteAnime.ToMalFavoriteAnime(user) as T,
				FavoriteCharacter favoriteCharacter => favoriteCharacter.ToMalFavoriteCharacter(user) as T,
				FavoriteManga favoriteManga         => favoriteManga.ToMalFavoriteManga(user) as T,
				FavoritePerson favoritePerson       => favoritePerson.ToMalFavoritePerson(user) as T,
				_                                   => throw new InvalidOperationException()
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

		internal static MalFavoriteCharacter ToMalFavoriteCharacter(this FavoriteCharacter character, MalUser user) => new()
		{
			Id = character.Url.Id,
			Name = character.Name,
			ImageUrl = character.ImageUrl,
			NameUrl = character.Url.Url,
			FromTitleName = character.FromName,
			FromTitleUrl = character.FromUrl.Url,
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

		internal static DiscordEmbedBuilder ToDiscordEmbedBuilder(this IMalFavorite favorite, bool added)
		{
			var eb = new DiscordEmbedBuilder
			{
				Url = favorite.NameUrl
			}.WithThumbnail(favorite.ImageUrl).AddField("Status", $"{(added ? "Added" : "Removed")} favorite", true);

			eb.WithColor(added ? Constants.MalGreen : Constants.MalRed);

			var title = "";
			if (favorite is IMalListFavorite baseListFavorite)
				title = $"{baseListFavorite.Name} ({baseListFavorite.Type}) [{baseListFavorite.StartYear.ToString()}]";
			else
				title = favorite.Name;
			eb.WithTitle(title);

			if (favorite is MalFavoriteCharacter favoriteCharacter)
				eb.AddField("From", Formatter.MaskedUrl(favoriteCharacter.FromTitleName, new(favoriteCharacter.FromTitleUrl)), true);

			return eb;
		}

		internal static DiscordEmbedBuilder WithMalUpdateProviderFooter(this DiscordEmbedBuilder builder)
		{
			builder.Footer = MalUpdateFooter;
			return builder;
		}

		internal static string ToHashString(this (string, string) v) => $"{v.Item1} {v.Item2}";
		internal static DiscordEmbedBuilder ToDiscordEmbedBuilder(this IListEntry listEntry, User user, DateTimeOffset timestamp)
		{
			static string TitleMediaTypeString(string title, string mediaType)
			{
				return title.EndsWith(mediaType) || title.EndsWith($"({mediaType})") ? title : $"{title} ({mediaType})";
			}

			var eb = new DiscordEmbedBuilder().WithUrl(listEntry.Url).WithThumbnail(listEntry.ImageUrl)
											  .WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl).WithTimestamp(timestamp);


			if (listEntry.Score != 0)
			{
				eb.AddField("Score", listEntry.Score.ToString(), true);
			}

			var userProgressText = listEntry switch
			{
				AnimeListEntry animeListEntry =>
					$"{animeListEntry.UserAnimeProgress.Humanize()} - {animeListEntry.WatchedEpisodes.ToString()} of {"ep".ToQuantity(animeListEntry.TotalEpisodes)}",
				MangaListEntry mangaListEntry =>
					$"{mangaListEntry.UserMangaProgress.Humanize()} - {mangaListEntry.ReadChapters.ToString()} of {"ch".ToQuantity(mangaListEntry.TotalChapters)}, {mangaListEntry.ReadVolumes.ToString()} of {"v".ToQuantity(mangaListEntry.TotalVolumes)}",

				_ =>
					$"{listEntry.UserProgress.Humanize()} - {listEntry.ProgressedSubEntries.ToString()} of {listEntry.TotalSubEntries.ToString()}"
			};

			eb.AddField("Progress", userProgressText, true);

			var entryStatus = listEntry switch
			{
				AnimeListEntry animeListEntry => animeListEntry.AnimeAiringStatus.Humanize(),
				MangaListEntry mangaListEntry => mangaListEntry.MangaPublishingStatus.Humanize(),
				_                             => listEntry.Status.Humanize(),
			};
			var title = "";
			if (listEntry.Title.Length + listEntry.MediaType.Length + entryStatus.Length <= 256)
				title = $"{TitleMediaTypeString(listEntry.Title, listEntry.MediaType)} [{entryStatus}]";
			else if (listEntry.Title.Length + listEntry.MediaType.Length <= 256)
				title = TitleMediaTypeString(listEntry.Title, listEntry.MediaType);
			else if (listEntry.Title.Length <= 256)
				title = listEntry.Title;
			else title = listEntry.Title.Substring(0, 256);

			if (listEntry.Tags.Length != 0)
			{
				if (listEntry.Tags.Length <= 1024)
					eb.AddField("Tags", listEntry.Tags, true);
				else
					eb.WithDescription($"Tags\n{listEntry.Tags}".Truncate(2048, Truncator.FixedNumberOfCharacters));
			}


			eb.WithTitle(title);
			eb.WithColor(Colors[listEntry.UserProgress]);
			return eb;
		}
	}
}