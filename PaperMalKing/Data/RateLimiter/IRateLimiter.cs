using System.Threading.Tasks;

namespace PaperMalKing.Data.RateLimiter
{
	public interface IRateLimiter
	{
		Task TickAsync();
	}
}