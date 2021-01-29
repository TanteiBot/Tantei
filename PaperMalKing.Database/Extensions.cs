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

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.Database.Models.Shikimori;

namespace PaperMalKing.Database
{
	public static class Extensions
	{
		public static async Task<int> SaveChangesAndThrowOnNoneAsync(this DbContext context, CancellationToken cancellationToken = default)
		{
			var rows = await context.SaveChangesAsync(cancellationToken);
			if (rows <= 0)
				throw new NoChangesSavedException(context);
			return rows;
		}

		public static MalUserFeatures GetDefault(this MalUserFeatures _) => MalUserFeatures.AnimeList | MalUserFeatures.MangaList   |
																			MalUserFeatures.Favorites | MalUserFeatures.Mention     |
																			MalUserFeatures.Website   | MalUserFeatures.MediaFormat |
																			MalUserFeatures.MediaStatus;

		public static ShikiUserFeatures GetDefault(this ShikiUserFeatures _) => ShikiUserFeatures.AnimeList  | ShikiUserFeatures.MangaList   |
																				ShikiUserFeatures.Favourites | ShikiUserFeatures.Mention     |
																				ShikiUserFeatures.Website    | ShikiUserFeatures.MediaFormat |
																				ShikiUserFeatures.MediaStatus;
	}
}