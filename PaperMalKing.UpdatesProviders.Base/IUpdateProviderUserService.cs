using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaperMalKing.UpdatesProviders.Base
{
	public interface IUpdateProviderUserService
	{
		string Name { get; }

		Task<IUser> AddUserAsync(string username, ulong userId, ulong guildId);

		Task<IUser> RemoveUserAsync(ulong userId);

		IAsyncEnumerable<IUser> ListUsersAsync(ulong guildId);
	}
}