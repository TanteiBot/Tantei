// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DSharpPlus.Entities;
using PaperMalKing.Common;
using PaperMalKing.Common.Exceptions;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don\'t access instance data should be static", Justification = "False positive")]
[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
internal static class Extensions
{
	extension(MalUserFeatures features)
	{
		public ParserOptions ToParserOptions()
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

		public TRequestOptions ToRequestOptions<TRequestOptions>()
			where TRequestOptions : unmanaged, Enum, allows ref struct
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
				var mangaFields = Unsafe.BitCast<AnimeFieldsToRequest, MangaFieldsToRequest>(fields);
				mangaFields |= MangaFieldsToRequest.Authors;
				return Unsafe.BitCast<MangaFieldsToRequest, TRequestOptions>(mangaFields);
			}

			if (features.HasFlag(MalUserFeatures.Studio))
			{
				fields |= AnimeFieldsToRequest.Studio;
			}

			return Unsafe.BitCast<AnimeFieldsToRequest, TRequestOptions>(fields);
		}
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
			_ => InvalidOperationException.Throw<T>(),
		}

		?? InvalidOperationException.Throw<T>();
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

	public static DiscordEmbedBuilder ToDiscordEmbedBuilder(this BaseMalFavorite favorite, bool added, MalUser dbUser)
	{
		var eb = new DiscordEmbedBuilder
		{
			Url = favorite.NameUrl,
		}.WithThumbnail(favorite.ImageUrl).WithDescription($"{(added ? "Added" : "Removed")} favorite");

		var color = dbUser.Colors.Find(added
			? static c => c.UpdateType == (byte)MalUpdateType.FavoriteAdded
			: static c => c.UpdateType == (byte)MalUpdateType.FavoriteRemoved)?.ColorValue ?? (added ? Constants.MalGreen : Constants.MalRed);

		eb.WithColor(color);

		var title = favorite is BaseMalListFavorite baseListFavorite
			? $"{baseListFavorite.Name} ({baseListFavorite.Type}) [{baseListFavorite.StartYear}]"
			: favorite.Name;

		eb.WithTitle(title);

		if (favorite is MalFavoriteCharacter favoriteCharacter)
		{
			eb.AddFieldIfPresent("From", favoriteCharacter.FromTitleName, inline: true);
		}

		return eb;
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