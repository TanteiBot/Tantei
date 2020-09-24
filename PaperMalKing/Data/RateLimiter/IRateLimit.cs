namespace PaperMalKing.Data.RateLimiter
{
	public interface IRateLimit
	{
		int AmountOfRequests { get; }
		long TimeConstraint { get; }
	}
}