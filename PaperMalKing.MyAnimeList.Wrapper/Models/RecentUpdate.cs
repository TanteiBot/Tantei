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

using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

namespace PaperMalKing.MyAnimeList.Wrapper.Models
{
	internal sealed class RecentUpdate
	{
		private (string inRssHash, string inProfileHash)? _hash = null;
		internal ListEntryType ListType { get; init; }

		internal int Id { get; init; }

		internal DateTimeOffset UpdateDateTime { get; init; }

		internal GenericProgress ProgressValue { get; init; }

		internal int ProgressedEntries { get; init; }

		internal (string inRssHash, string inProfileHash) Hash => this._hash ??= Extensions.GetHash(this.Id, this.ProgressedEntries, this.ProgressValue, 0);

		internal RecentUpdate(ListEntryType listType, int id, DateTimeOffset updateDateTime, GenericProgress progressValue, int progressedEntries)
		{
			this.ListType = listType;
			this.Id = id;
			this.UpdateDateTime = updateDateTime;
			this.ProgressValue = progressValue;
			this.ProgressedEntries = progressedEntries;
		}
	}
}