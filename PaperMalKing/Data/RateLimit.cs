using System;

namespace PaperMalKing.Data
{
	public sealed class RateLimit
	{
		public readonly int AmountOfRequests;

		public readonly TimeSpan TimeConstraint;
		
		public RateLimit(int amountOfRequests, TimeSpan timeConstraint)
		{
			this.AmountOfRequests = amountOfRequests;
			this.TimeConstraint = timeConstraint;
		}
	}
}