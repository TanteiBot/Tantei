using System.Threading.Tasks;

namespace PaperMalKing.Providers.Base
{
	public interface IUserManager
	{
		/// <summary>
		/// Add user to updates provider
		/// </summary>
		/// <param name="username">User's name on updates provider</param>
		/// <param name="discordId">User's Discord Id</param>
		/// <returns></returns>
		Task AddUser(string username, ulong discordId);

		/// <summary>
		/// Add user to updates provider
		/// </summary>
		/// <param name="userId">User's id on updates provider</param>
		/// <param name="discordId">User's Discord Id</param>
		/// <returns></returns>
		Task AddUser(ulong userId, ulong discordId);

		/// <summary>
		/// Removes user from update provider
		/// </summary>
		/// <param name="userId">User's Discord Id</param>
		/// <returns></returns>
		Task RemoveUser(ulong userId);
	}
}