#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

namespace PaperMalKing.Common.RateLimiters
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

		public RateLimit(int amountOfRequests, TimeSpan period) : this(amountOfRequests, (int)period.TotalMilliseconds)
		{ }

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{this.AmountOfRequests.ToString()}r per {this.PeriodInMilliseconds.ToString()}ms";
		}
	}
}