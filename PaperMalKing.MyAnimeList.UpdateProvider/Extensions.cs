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
			}.WithThumbnail(favorite.ImageUrl).WithDescription($"{(added ? "Added" : "Removed")} favorite");

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
			static string TitleMediaTypeString(string title, string mediaType) =>
				title.EndsWith(mediaType) || title.EndsWith($"({mediaType})") ? title : $"{title} ({mediaType})";

			var eb = new DiscordEmbedBuilder().WithThumbnail(listEntry.ImageUrl).WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl)
											  .WithTimestamp(timestamp);


			if (listEntry.Score != 0)
				eb.AddField("Score", listEntry.Score.ToString(), true);

			var userProgressText = listEntry switch
			{
				AnimeListEntry
					{UserAnimeProgress: AnimeProgress.PlanToWatch, TotalEpisodes: 0, WatchedEpisodes: 0} ale => ale.UserAnimeProgress.Humanize(
					LetterCasing.Sentence),
				AnimeListEntry
						{UserAnimeProgress: AnimeProgress.PlanToWatch, WatchedEpisodes: 0} ale =>
					$"{ale.UserAnimeProgress.Humanize(LetterCasing.Sentence)} - {ale.TotalEpisodes.ToString()} ep.",
				AnimeListEntry ale =>
					$"{ale.UserAnimeProgress.Humanize(LetterCasing.Sentence)} - {(ale.TotalEpisodes == ale.WatchedEpisodes ? $"{ale.TotalEpisodes.ToString()} ep." : $"{ale.WatchedEpisodes.ToString()}/{ale.TotalEpisodes.ToString()} ep.")}",

				MangaListEntry
					{UserMangaProgress: MangaProgress.PlanToRead, TotalChapters: 0, ReadChapters: 0} mle => mle.UserMangaProgress.Humanize(
					LetterCasing.Sentence),
				MangaListEntry
						{UserMangaProgress: MangaProgress.PlanToRead, ReadChapters: 0} mle =>
					$"{mle.UserMangaProgress.Humanize(LetterCasing.Sentence)} - {mle.TotalChapters.ToString()} ch, {mle.TotalVolumes.ToString()} v.",
				MangaListEntry mle =>
					$"{mle.UserMangaProgress.Humanize(LetterCasing.Sentence)} - {mle.ReadChapters.ToString()}/{mle.TotalChapters.ToString()} ch, {mle.ReadVolumes.ToString()}/{mle.TotalVolumes.ToString()} v.",

				_ =>
					$"{listEntry.UserProgress.Humanize(LetterCasing.Sentence)} - {listEntry.ProgressedSubEntries.ToString()}/{listEntry.TotalSubEntries.ToString()}"
			};

			eb.AddField("Progress", userProgressText, true);

			var entryStatus = listEntry switch
			{
				AnimeListEntry animeListEntry => animeListEntry.AnimeAiringStatus.Humanize(LetterCasing.Sentence),
				MangaListEntry mangaListEntry => mangaListEntry.MangaPublishingStatus.Humanize(LetterCasing.Sentence),
				_                             => listEntry.Status.Humanize(LetterCasing.Sentence),
			};
			var title = $"{TitleMediaTypeString(listEntry.Title, listEntry.MediaType)} [{entryStatus}]";

			if (title.Length <= 256)
			{
				eb.Url = listEntry.Url;
				eb.Title = title;
			}
			else
				eb.Description = title;

			if (listEntry.Tags.Length != 0)
			{
				if (listEntry.Tags.Length <= 1024)
					eb.AddField("Tags", listEntry.Tags, true);
				else
				{
					var l = eb.Description?.Length ?? 0;
					var descToAdd = $"Tags\n{listEntry.Tags}".Truncate(2048 - l - 1, Truncator.FixedNumberOfCharacters);
					if (string.IsNullOrEmpty(eb.Description))
						eb.WithDescription(descToAdd);
					else
						eb.Description += descToAdd;
				}
			}


			eb.WithTitle(title);
			eb.WithColor(Colors[listEntry.UserProgress]);
			return eb;
		}
	}
}