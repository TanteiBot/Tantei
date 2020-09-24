using System;

namespace PaperMalKing.Data.RateLimiter
{
	public sealed class RateLimit : IRateLimit
	{
		public int AmountOfRequests { get; private set; }

		public long TimeConstraint { get; private set; }

		public RateLimit(int amountOfRequests, TimeSpan timeConstraint)
		{
			if (amountOfRequests <= 0)
				throw new ArgumentOutOfRangeException(nameof(amountOfRequests));
			this.AmountOfRequests = amountOfRequests;
			this.TimeConstraint = timeConstraint.Duration().Milliseconds;
		}
	}
}