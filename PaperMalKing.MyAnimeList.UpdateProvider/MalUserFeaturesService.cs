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

using System.Reflection;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Attributes;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	internal sealed class MalUserFeaturesService : IUserFeaturesService<MalUserFeatures>
	{
		private readonly MyAnimeListClient _client;
		private readonly ILogger<MalUserFeaturesService> _logger;
		private readonly IServiceProvider _serviceProvider;


		public MalUserFeaturesService(MyAnimeListClient client, ILogger<MalUserFeaturesService> logger, IServiceProvider serviceProvider)
		{
			this._client = client;
			this._logger = logger;
			this._serviceProvider = serviceProvider;
			var t = typeof(MalUserFeatures);
			var ti = t.GetTypeInfo();
			var values = Enum.GetValues(t).Cast<MalUserFeatures>().Where(v => v != MalUserFeatures.None);

			foreach (var enumVal in values)
			{
				var name = enumVal.ToString();
				var fieldVal = ti.DeclaredMembers.First(xm => xm.Name == name);
				var attribute = fieldVal!.GetCustomAttribute<FeatureDescriptionAttribute>()!;

				this.Descriptions[enumVal] = (attribute.Description, attribute.Summary);
			}
		}

		public Dictionary<MalUserFeatures, (string, string)> Descriptions { get; } = new();

		IReadOnlyDictionary<MalUserFeatures, (string, string)> IUserFeaturesService<MalUserFeatures>.Descriptions => this.Descriptions;


		public async Task EnableFeaturesAsync(IReadOnlyList<MalUserFeatures> features, ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var dbUser = await db.MalUsers
								 .Include(mu => mu.DiscordUser)
								 .Include(u => u.FavoriteAnimes)
								 .Include(u => u.FavoriteMangas)
								 .Include(u => u.FavoriteCharacters)
								 .Include(u => u.FavoritePeople)
								 .FirstOrDefaultAsync(u => u.DiscordUser.DiscordUserId == userId).ConfigureAwait(false);
			if (dbUser == null)
				throw new UserFeaturesException("You must register first before enabling features");
			var total = features.Aggregate((acc, next) => acc | next);
			User? user = null;
			dbUser.Features |= total;
			var now = DateTimeOffset.UtcNow;
			for (var index = 0; index < features.Count; index++)
			{
				var feature = features[index];
				switch (feature)
				{
					case MalUserFeatures.AnimeList:
						{
							user ??= await this._client.GetUserAsync(dbUser.Username, total.ToParserOptions(), CancellationToken.None).ConfigureAwait(false);
							dbUser.LastAnimeUpdateHash = user.LatestAnimeUpdate?.Hash.ToHashString() ?? "";
							dbUser.LastUpdatedAnimeListTimestamp = now;
							break;
						}
					case MalUserFeatures.MangaList:
						{
							user ??= await this._client.GetUserAsync(dbUser.Username, total.ToParserOptions(), CancellationToken.None).ConfigureAwait(false);
							dbUser.LastAnimeUpdateHash = user.LatestAnimeUpdate?.Hash.ToHashString() ?? "";
							dbUser.LastUpdatedMangaListTimestamp = now;
							break;
						}
					case MalUserFeatures.Favorites:
						{
							user ??= await this._client.GetUserAsync(dbUser.Username, total.ToParserOptions(), CancellationToken.None).ConfigureAwait(false);
							dbUser.FavoriteAnimes = user.Favorites.FavoriteAnime.Select(x => x.ToMalFavoriteAnime(dbUser)).ToList();
							dbUser.FavoriteMangas = user.Favorites.FavoriteManga.Select(x => x.ToMalFavoriteManga(dbUser)).ToList();
							dbUser.FavoriteCharacters = user.Favorites.FavoriteCharacters.Select(x => x.ToMalFavoriteCharacter(dbUser)).ToList();
							dbUser.FavoritePeople = user.Favorites.FavoritePeople.Select(x => x.ToMalFavoritePerson(dbUser)).ToList();
							break;
						}
				}
			}

			db.MalUsers.Update(dbUser);
			await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
		}

		public async Task DisableFeaturesAsync(IReadOnlyList<MalUserFeatures> features, ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var dbUser = await db.MalUsers
								 .Include(mu => mu.DiscordUser)
								 .Include(u => u.FavoriteAnimes)
								 .Include(u => u.FavoriteMangas)
								 .Include(u => u.FavoriteCharacters)
								 .Include(u => u.FavoritePeople)
								 .FirstOrDefaultAsync(u => u.DiscordUser.DiscordUserId == userId).ConfigureAwait(false);
			if (dbUser == null)
				throw new UserFeaturesException("You must register first before disabling features");

			var total = features.Aggregate((acc, next) => acc | next);

			dbUser.Features &= ~total;
			if (features.Any(x => x == MalUserFeatures.Favorites))
			{
				dbUser.FavoriteAnimes.Clear();
				dbUser.FavoriteMangas.Clear();
				dbUser.FavoriteCharacters.Clear();
				dbUser.FavoritePeople.Clear();
			}

			db.MalUsers.Update(dbUser);
			await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
		}

		public async Task<string> EnabledFeaturesAsync(ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var dbUser = await db.MalUsers.Include(mu => mu.DiscordUser).AsNoTrackingWithIdentityResolution()
								 .FirstOrDefaultAsync(u => u.DiscordUser.DiscordUserId == userId).ConfigureAwait(false);
			if (dbUser == null)
				throw new UserFeaturesException("You must register first before checking for enabled features");

			return dbUser.Features.Humanize();
		}
	}
}