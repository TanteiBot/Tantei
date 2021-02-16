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

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	internal abstract class BaseListSubEntry
	{
		protected abstract string Type { get; }

		public abstract string TotalAmount { get; }

		[JsonPropertyName("id")]
		public ulong Id { get; init; }

		[JsonPropertyName("name")]
		public string Name { get; init; } = null!;

		[JsonPropertyName("kind")]
		public string Kind { get; init; } = null!;

		[JsonPropertyName("status")]
		public SubEntryReleasingStatus Status { get; init; }

		public string Url => Utils.GetUrl(this.Type, this.Id);

		public string ImageUrl => Utils.GetImageUrl(this.Type, this.Id);
	}
}