using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiter
{
	public interface IRateLimiter
	{
		Task TickAsync();
	}
}