using System;

namespace PaperMalKing.Common.RateLimiter
{
	public sealed class RateLimit
	{
		public readonly int AmountOfRequests;

		public readonly long TimeConstraint;

		public RateLimit(int amountOfRequests, TimeSpan timeConstraint)
		{
			if (amountOfRequests <= 0)
				throw new ArgumentOutOfRangeException(nameof(amountOfRequests));
			this.AmountOfRequests = amountOfRequests;
			this.TimeConstraint = timeConstraint.Duration().Milliseconds;
		}
	}
}