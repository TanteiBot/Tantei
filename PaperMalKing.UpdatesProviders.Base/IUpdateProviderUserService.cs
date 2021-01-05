using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaperMalKing.UpdatesProviders.Base
{
	public interface IUpdateProviderUserService
	{
		string Name { get; }

		Task<BaseUser> AddUserAsync(string username, ulong userId, ulong guildId);

		Task<BaseUser> RemoveUserAsync(ulong userId);

		Task RemoveUserHereAsync(ulong userId, ulong guildId);

		IAsyncEnumerable<BaseUser> ListUsersAsync(ulong guildId);
	}
}