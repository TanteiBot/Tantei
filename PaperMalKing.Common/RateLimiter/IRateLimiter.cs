using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiter
{
	public interface IRateLimiter<T>
	{
		RateLimit RateLimit { get; }

		Task TickAsync();
	}
}