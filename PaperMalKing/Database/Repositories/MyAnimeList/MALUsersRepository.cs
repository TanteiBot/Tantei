using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PaperMalKing.Database.Models.MyAnimeList;

namespace PaperMalKing.Database.Repositories.MyAnimeList
{
	public sealed class MALUsersRepository : BaseRepository
	{
		private readonly DbSet<MALUser> _users;

		/// <inheritdoc />
		public MALUsersRepository(DatabaseContext context) : base(context)
		{
			this._users = context.MyAnimeListUsers;
		}

		public ConfiguredTaskAwaitable<MALUser> GetUserByIdAsync(long userId, bool includeFavoritesAndColors = true)
		{
			IQueryable<MALUser> users = this._users;
			if (includeFavoritesAndColors)
			{
				users = this.IncludeFavoritesAndColors();
			}
			return users.FirstOrDefaultAsync(u => u.UserId == userId).ConfigureAwait(false)!;
		}

		public ConfiguredTaskAwaitable<MALUser> GetUserByDiscordIdAsync(long discordId, bool includeFavoritesAndColors = true)
		{
			IQueryable<MALUser> users = this._users;

			if (includeFavoritesAndColors)
			{
				users = this.IncludeFavoritesAndColors();
			}
			return users.FirstOrDefaultAsync(u => u.DiscordUserId == discordId).ConfigureAwait(false)!;
		}

		public ConfiguredTaskAwaitable<MALUser[]> GetUsers(bool includeFavoritesAndColors = true)
		{
			IQueryable<MALUser> users = this._users;

			if (includeFavoritesAndColors)
			{
				users = this.IncludeFavoritesAndColors();
			}
			return users.ToArrayAsync().ConfigureAwait(false)!;
		}

		public void AddUser(MALUser user) => this._users.Add(user);

		public void UpdateUser(MALUser user) => this._users.Update(user);

		public void RemoveUser(MALUser user) => this._users.Remove(user);
		
		private IQueryable<MALUser> IncludeFavoritesAndColors() => this._users
																	   .Include(u => u.FavoriteAnimes)
																	   .Include(u => u.FavoriteMangas)
																	   .Include(u => u.FavoriteCharacters)
																	   .Include(u => u.FavoritePersons)
																	   .Include(u => u.AnimeListColors)
																	   .Include(u => u.MangaListColors);

		public async Task RemoveUserAsync(long discordId)
		{
			var user = await this.GetUserByDiscordIdAsync(discordId, false);
			if (user == null) return;
			this._users.Remove(user);
		}
	}
}