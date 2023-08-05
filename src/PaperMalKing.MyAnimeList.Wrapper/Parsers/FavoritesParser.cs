// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AngleSharp.Dom;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;
[SuppressMessage("Design", "MA0048:File name must match type name")]
[SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members")]
internal static partial class UserProfileParser
{
	private static class FavoritesParser
	{
		public static UserFavorites Parse(IDocument document)
		{
			var animes = ParseFavoriteAnime(document);
			var manga = ParseFavoriteManga(document);
			var characters = ParseFavoriteCharacter(document);
			var people = ParseFavoritePerson(document);
			var companies = ParseFavoriteCompanies(document);
			return new()
			{
				FavoriteAnime = animes,
				FavoriteManga = manga,
				FavoriteCharacters = characters,
				FavoritePeople = people,
				FavoriteCompanies = companies,
			};
		}

		private static IReadOnlyList<FavoriteCompany> ParseFavoriteCompanies(IDocument parent)
		{
			var companyFavoritesNodes = GetFavoritesNodes(parent, "company_favorites");
			if (companyFavoritesNodes is null)
			{
				return Array.Empty<FavoriteCompany>();
			}

			return Parse(companyFavoritesNodes, node => new FavoriteCompany(ParseBaseFavorite(node)));
		}

		private static IReadOnlyList<FavoriteAnime> ParseFavoriteAnime(IDocument parent)
		{
			var animeFavoritesNodes = GetFavoritesNodes(parent, "anime_favorites");
			if (animeFavoritesNodes is null)
			{
				return Array.Empty<FavoriteAnime>();
			}
			return Parse(animeFavoritesNodes,
				node => new FavoriteAnime(ParseListEntryFavorite(node)));
		}

		private static IReadOnlyList<FavoriteManga> ParseFavoriteManga(IDocument parent)
		{
			var mangaFavoriteNodes = GetFavoritesNodes(parent, "manga_favorites");
			if (mangaFavoriteNodes is null)
			{
				return Array.Empty<FavoriteManga>();
			}
			return Parse(mangaFavoriteNodes,
				node => new FavoriteManga(ParseListEntryFavorite(node)));
		}

		private static IReadOnlyList<FavoriteCharacter> ParseFavoriteCharacter(IDocument parent)
		{
			var characterFavoriteNodes = GetFavoritesNodes(parent, "character_favorites");
			if (characterFavoriteNodes is null)
			{
				return Array.Empty<FavoriteCharacter>();
			}
			return Parse(characterFavoriteNodes, node =>
			{
				var baseFav = ParseBaseFavorite(node);
				var fromNode = node.QuerySelector(".users")!;
				return new FavoriteCharacter(fromNode.TextContent.Trim(), baseFav);
			});
		}

		private static IReadOnlyList<FavoritePerson> ParseFavoritePerson(IDocument parent)
		{
			var personFavoriteNodes = GetFavoritesNodes(parent, "person_favorites");
			if (personFavoriteNodes is null)
			{
				return Array.Empty<FavoritePerson>();
			}
			return Parse(personFavoriteNodes, node => new FavoritePerson(ParseBaseFavorite(node)));
		}

		private static IHtmlCollection<IElement>? GetFavoritesNodes(IDocument parent, string sectionName)
		{
			return parent.QuerySelectorAll($"div#{sectionName} div.fav-slide-outer > ul > li.btn-fav > a");
		}

		private static IReadOnlyList<TF> Parse<TF>(IHtmlCollection<IElement> elements, Func<IElement, TF> func)
			where TF : BaseFavorite
		{
			var favs = elements.Length == 0 ? Array.Empty<TF>() : new TF[elements.Length];
			for (var i = 0; i < favs.Length; i++)
				favs[i] = func(elements[i]);

			return favs;
		}

		private static BaseListFavorite ParseListEntryFavorite(IElement parent)
		{
			var baseFav = ParseBaseFavorite(parent);

			var typeYearNode = parent.QuerySelector(".users")!;
			var text = typeYearNode.TextContent.AsSpan();
			var index = text.IndexOf(Constants.DOT, StringComparison.Ordinal);

			return new(text[..index].ToString(), ushort.Parse(text[(index + 1)..], NumberFormatInfo.InvariantInfo), baseFav);
		}

		private static BaseFavorite ParseBaseFavorite(IElement parent)
		{
			var urlUnparsed = parent.GetAttribute("href")!;
			if (urlUnparsed.StartsWith('/'))
			{
				urlUnparsed = Constants.BASE_URL + urlUnparsed;
			}

			var titleNode = parent.QuerySelector(".title")!;
			var imageUrlNode = parent.QuerySelector(".image")!;
			return new BaseFavorite(new MalUrl(urlUnparsed), titleNode.TextContent, imageUrlNode.GetAttribute("data-src")!.Replace("/r/140x220", "", StringComparison.OrdinalIgnoreCase));
		}
	}
}