using System;

namespace PaperMalKing.Common.RateLimiter
{
	public sealed class RateLimit
	{
		public long AmountOfRequests { get; }

		public long PeriodInMilliseconds { get; }

		public RateLimit(int amountOfRequests, int periodInMilliseconds)
		{
			if (amountOfRequests <= 0)
				throw new ArgumentException("Amount of requests must be a number bigger than 0", nameof(amountOfRequests));
			if (periodInMilliseconds <= 0)
				throw new ArgumentException("Period of time in milliseconds must be bigger than 0", nameof(periodInMilliseconds));
			this.AmountOfRequests = amountOfRequests;
			this.PeriodInMilliseconds = periodInMilliseconds;
		}

		public RateLimit(int amountOfRequests, TimeSpan period) : this(amountOfRequests, (int) period.TotalMilliseconds)
		{ }

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{this.AmountOfRequests.ToString()}r per {this.PeriodInMilliseconds.ToString()}ms";
		}
	}
}