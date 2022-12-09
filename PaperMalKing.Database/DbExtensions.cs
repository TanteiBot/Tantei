// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.Database.Models.Shikimori;

namespace PaperMalKing.Database
{
	public static class DbExtensions
	{
		public static async Task<int> SaveChangesAndThrowOnNoneAsync(this DbContext context, CancellationToken cancellationToken = default)
		{
			var rows = await context.TrySaveChangesUntilDatabaseIsUnlockedAsync(cancellationToken).ConfigureAwait(false);
			if (rows <= 0)
				throw new NoChangesSavedException(context);
			return rows;
		}

		public static async Task<int> TrySaveChangesUntilDatabaseIsUnlockedAsync(this DbContext context, CancellationToken cancellationToken = default)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var rows = context.SaveChanges();
					return rows;
				}
				catch (SqliteException ex) when (ex.SqliteErrorCode == 5) // Database is locked
				{
					await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
				}
			}
			throw new TaskCanceledException("Saving changes were cancelled");
		}

		public static MalUserFeatures GetDefault(this MalUserFeatures _) => MalUserFeatures.AnimeList | MalUserFeatures.MangaList   |
																			MalUserFeatures.Favorites | MalUserFeatures.Mention     |
																			MalUserFeatures.Website   | MalUserFeatures.MediaFormat |
																			MalUserFeatures.MediaStatus;

		public static ShikiUserFeatures GetDefault(this ShikiUserFeatures _) => ShikiUserFeatures.AnimeList  | ShikiUserFeatures.MangaList   |
																				ShikiUserFeatures.Favourites | ShikiUserFeatures.Mention     |
																				ShikiUserFeatures.Website    | ShikiUserFeatures.MediaFormat |
																				ShikiUserFeatures.MediaStatus;

		public static AniListUserFeatures GetDefault(this AniListUserFeatures _) => AniListUserFeatures.AnimeList  | AniListUserFeatures.MangaList   |
																					AniListUserFeatures.Favourites | AniListUserFeatures.Mention     |
																					AniListUserFeatures.Website    | AniListUserFeatures.MediaFormat |
																					AniListUserFeatures.MediaStatus;
	}
}