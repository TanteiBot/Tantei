#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
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

using HtmlAgilityPack;
using PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static partial class UserProfileParser
{
	private static class FavoritesParser
	{
		internal static UserFavorites Parse(HtmlNode node)
		{
			var favDiv = node.SelectNodes("//div[contains(@class, 'user-favorites ')]/div");
			var animes = ParseFavoriteAnime(favDiv[0]);
			var manga = ParseFavoriteManga(favDiv[1]);
			var characters = ParseFavoriteCharacter(favDiv[2]);
			var people = ParseFavoritePerson(favDiv[3]);
			return new()
			{
				FavoriteAnime = animes,
				FavoriteManga = manga,
				FavoriteCharacters = characters,
				FavoritePeople = people
			};
		}

		private static IReadOnlyList<FavoriteAnime> ParseFavoriteAnime(HtmlNode parent)
		{
			return Parse(parent, "//ul[contains(@class,'anime')]/li",
				node => new FavoriteAnime(ParseListEntryFavorite(node)));
		}

		private static IReadOnlyList<FavoriteManga> ParseFavoriteManga(HtmlNode parent)
		{
			return Parse(parent, "//ul[contains(@class,'manga')]/li",
				node => new FavoriteManga(ParseListEntryFavorite(node)));
		}

		private static IReadOnlyList<FavoriteCharacter> ParseFavoriteCharacter(HtmlNode parent)
		{
			return Parse(parent, "//ul[contains(@class,'characters')]/li", node =>
			{
				var hd = new HtmlDocument();
				hd.LoadHtml(node.InnerHtml);
				node = hd.DocumentNode;
				var baseFav = ParseBaseFavorite(node);
				var fromNode = node.SelectSingleNode("//div[position() = 2]/span/a");
				return new FavoriteCharacter(new(Constants.BASE_URL + fromNode.Attributes["href"].Value),
					fromNode.InnerText.Trim(), baseFav);
			});
		}

		private static IReadOnlyList<FavoritePerson> ParseFavoritePerson(HtmlNode parent)
		{
			return Parse(parent, "//ul[contains(@class,'people')]/li",
				node => new FavoritePerson(ParseBaseFavorite(node)));
		}

		private static IReadOnlyList<TF> Parse<TF>(HtmlNode parent, string xpath, Func<HtmlNode, TF> func)
			where TF : BaseFavorite
		{
			var nodes = parent.SelectNodes(xpath);
			var favs = nodes?.Count == null || nodes?.Count == 0 ? Array.Empty<TF>() : new TF[nodes!.Count];
			for (var i = 0; i < favs.Length; i++)
				favs[i] = func(nodes![i]);

			return favs;
		}

		private static BaseListFavorite ParseListEntryFavorite(HtmlNode parent)
		{
			var hd = new HtmlDocument();
			hd.LoadHtml(parent.InnerHtml);
			parent = hd.DocumentNode;
			var baseFav = ParseBaseFavorite(parent);

			var textNode = parent.SelectSingleNode("//div[position() = 2]/span");
			var splittedTypeYear = textNode.InnerText.Split(Constants.DOT);
			var type = splittedTypeYear[0].Trim();
			var year = int.Parse(splittedTypeYear[1].Trim());
			return new(type, year, baseFav);
		}

		private static BaseFavorite ParseBaseFavorite(HtmlNode parent)
		{
			var hd = new HtmlDocument();
			hd.LoadHtml(parent.InnerHtml);
			parent = hd.DocumentNode;
			var imageNode = parent.SelectSingleNode("//div[position() = 1]/a");
			var imageUrl = imageNode.SelectSingleNode("//img").Attributes["src"].Value;
			var url = imageNode.Attributes["href"].Value;
			var name = parent.SelectSingleNode("//div[position() = 2]/a").InnerText;
			return new(new(url), name, imageUrl);
		}
	}
}