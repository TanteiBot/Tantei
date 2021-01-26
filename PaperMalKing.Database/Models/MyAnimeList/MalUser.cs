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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	public sealed class MalUser
	{
		public DiscordUser DiscordUser { get; init; } = null!;

		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Key]
		public int UserId { get; init; }

		public string Username { get; set; } = null!;

		public DateTimeOffset LastUpdatedAnimeListTimestamp { get; set; }

		public DateTimeOffset LastUpdatedMangaListTimestamp { get; set; }

		public string LastAnimeUpdateHash { get; set; } = null!;

		public string LastMangaUpdateHash { get; set; } = null!;
		
		public MalUserFeatures Features { get; init; }
		
		public List<MalFavoriteAnime> FavoriteAnimes { get; set; } = null!;

		public List<MalFavoriteManga> FavoriteMangas { get; set; } = null!;

		public List<MalFavoriteCharacter> FavoriteCharacters { get; set; } = null!;

		public List<MalFavoritePerson> FavoritePeople { get; set; } = null!;
	}
}