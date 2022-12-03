#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
	#pragma warning disable MA0048
	internal static partial class UserProfileParser
	#pragma warning restore MA0048
	{
		private static class FavoritesParser
		{
			internal static UserFavorites Parse(HtmlNode node)
			{
				var animes = ParseFavoriteAnime(node);
				var manga = ParseFavoriteManga(node);
				var characters = ParseFavoriteCharacter(node);
				var people = ParseFavoritePerson(node);
				var companies = ParseFavoriteCompanies(node);
				return new()
				{
					FavoriteAnime = animes,
					FavoriteManga = manga,
					FavoriteCharacters = characters,
					FavoritePeople = people,
					FavoriteCompanies = companies
				};
			}

			private static IReadOnlyList<FavoriteCompany> ParseFavoriteCompanies(HtmlNode parent)
			{
				var companyFavoritesNodes = GetFavoritesNodes(parent, "company_favorites");
				if (companyFavoritesNodes is null)
				{
					return Array.Empty<FavoriteCompany>();
				}

				return Parse(companyFavoritesNodes, node => new FavoriteCompany(ParseBaseFavorite(node)));
			}

			private static IReadOnlyList<FavoriteAnime> ParseFavoriteAnime(HtmlNode parent)
			{
				var animeFavoritesNodes = GetFavoritesNodes(parent, "anime_favorites");
				if (animeFavoritesNodes == null)
				{
					return Array.Empty<FavoriteAnime>();
				}
				return Parse(animeFavoritesNodes,
					node => new FavoriteAnime(ParseListEntryFavorite(node)));
			}

			private static IReadOnlyList<FavoriteManga> ParseFavoriteManga(HtmlNode parent)
			{
				var mangaFavoriteNodes = GetFavoritesNodes(parent, "manga_favorites");
				if (mangaFavoriteNodes == null)
				{
					return Array.Empty<FavoriteManga>();
				}
				return Parse(mangaFavoriteNodes,
					node => new FavoriteManga(ParseListEntryFavorite(node)));
			}

			private static IReadOnlyList<FavoriteCharacter> ParseFavoriteCharacter(HtmlNode parent)
			{
				var characterFavoriteNodes = GetFavoritesNodes(parent, "character_favorites");
				if (characterFavoriteNodes == null)
				{
					return Array.Empty<FavoriteCharacter>();
				}
				return Parse(characterFavoriteNodes, node =>
				{
					var baseFav = ParseBaseFavorite(node);
					var fromNode = 	node.ChildNodes.First(x => x.Name == "a").ChildNodes.First(x => x.HasClass("users"));
					return new FavoriteCharacter(fromNode.InnerText.Trim(), baseFav);
				});
			}

			private static IReadOnlyList<FavoritePerson> ParseFavoritePerson(HtmlNode parent)
			{
				var personFavoriteNodes = GetFavoritesNodes(parent, "person_favorites");
				if (personFavoriteNodes == null)
				{
					return Array.Empty<FavoritePerson>();
				}
				return Parse(personFavoriteNodes, node => new FavoritePerson(ParseBaseFavorite(node)));
			}

			private static HtmlNodeCollection? GetFavoritesNodes(HtmlNode node, string sectionName)
			{
				return node.SelectNodes($"//div[contains(@id, '{sectionName}')]/div/ul/li[contains(@class, 'btn-fav')]");
			}

			private static IReadOnlyList<TF> Parse<TF>(HtmlNodeCollection nodes, Func<HtmlNode, TF> func)
				where TF : BaseFavorite
			{
				var favs = nodes.Count == 0 ? Array.Empty<TF>() : new TF[nodes.Count];
				for (var i = 0; i < favs.Length; i++)
					favs[i] = func(nodes![i]);

				return favs;
			}

			private static BaseListFavorite ParseListEntryFavorite(HtmlNode parent)
			{
				var baseFav = ParseBaseFavorite(parent);
				var aNode = parent.ChildNodes.First(x => x.Name == "a");

				var typeYearNode = aNode.ChildNodes.First(x=> x.HasClass("users"));
				var strings = typeYearNode.InnerText.Split(Constants.DOT);

				return new(strings[0], int.Parse(strings[1]), baseFav);
			}

			private static BaseFavorite ParseBaseFavorite(HtmlNode parent)
			{
				var aNode = parent.ChildNodes.First(x => x.Name == "a");
				var urlUnparsed = aNode.GetAttributeValue("href", "");
				if (urlUnparsed.StartsWith("/", StringComparison.Ordinal))
				{
					urlUnparsed = Constants.BASE_URL + urlUnparsed;
				}

				var titleNode = aNode.ChildNodes.First(x=> x.HasClass("title"));
				var imageUrlNode = aNode.ChildNodes.First(x => x.HasClass("image"));
				return new BaseFavorite(new MalUrl(urlUnparsed), titleNode.InnerText, imageUrlNode.GetAttributeValue("data-src", "").Replace("/r/140x220", "", StringComparison.OrdinalIgnoreCase));
			}
		}
	}
}
