using System;

namespace PaperMalKing.Data
{
	public readonly struct RateLimiterToken
	{
		public readonly DateTime CreationDate;

		public RateLimiterToken(DateTime creationDate)
		{
			this.CreationDate = creationDate;
		}
	}
}