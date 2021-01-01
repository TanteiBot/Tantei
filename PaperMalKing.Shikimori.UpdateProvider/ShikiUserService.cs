using System.Collections.Generic;
using System.Threading.Tasks;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	internal sealed class ShikiUserService : IUpdateProviderUserService
	{
		/// <inheritdoc />
		public string Name => Constants.NAME;

		/// <inheritdoc />
		public async Task<IUser> AddUserAsync(string username, ulong userId, ulong guildId) => null;

		/// <inheritdoc />
		public async Task<IUser> RemoveUserAsync(ulong userId) => null;

		/// <inheritdoc />
		public IAsyncEnumerable<IUser> ListUsersAsync(ulong guildId) => null;
	}
}