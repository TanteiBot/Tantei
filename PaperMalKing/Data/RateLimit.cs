using System;

namespace PaperMalKing.Data
{
	public sealed class RateLimit
	{
		public readonly int AmountOfRequests;

		public readonly TimeSpan TimeConstraint;

		private readonly double _requestRestorationTime;

		public RateLimit(int amountOfRequests, TimeSpan timeConstraint)
		{
			this.AmountOfRequests = amountOfRequests;
			this.TimeConstraint = timeConstraint;
			this._requestRestorationTime = this.TimeConstraint.TotalMilliseconds / (this.AmountOfRequests * 1.0);
		}
	}
}