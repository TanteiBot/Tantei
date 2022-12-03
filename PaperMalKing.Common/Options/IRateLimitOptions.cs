#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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

namespace PaperMalKing.Common.Options
{
	public interface IRateLimitOptions<T>
	{
		/// <summary>
		/// Amount of requests that can be permitted
		/// </summary>
		int AmountOfRequests { get; init; }

		/// <summary>
		/// Amount of time in milliseconds between refreshing number of permits
		/// </summary>
		int PeriodInMilliseconds { get; init; }
	}
}