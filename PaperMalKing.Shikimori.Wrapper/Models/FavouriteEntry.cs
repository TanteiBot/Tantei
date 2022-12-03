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

using System;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models
{
	internal sealed class FavouriteEntry : IEquatable<FavouriteEntry>
	{
		[JsonIgnore]
		public string? GenericType { get; internal set; }

		[JsonIgnore]
		public string? SpecificType { get; internal set; } = null;

		[JsonPropertyName("id")]
		public ulong Id { get; init; }

		[JsonPropertyName("name")]
		public string Name { get; init; } = null!;

		[JsonPropertyName("russian")]
		public string? RussianName { get; init; } = null!;

		public string? ImageUrl => Utils.GetImageUrl(this.GenericType!, this.Id);

		public string? Url => Utils.GetUrl(this.GenericType!, this.Id);

		/// <inheritdoc />
		public bool Equals(FavouriteEntry? other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return this.GenericType == other.GenericType && this.Id == other.Id;
		}

		/// <inheritdoc />
		public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FavouriteEntry other && this.Equals(other);

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return ((this.GenericType != null ? this.GenericType.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0) * 397) ^ this.Id.GetHashCode();
			}
		}

		public static bool operator ==(FavouriteEntry? left, FavouriteEntry? right) => Equals(left, right);

		public static bool operator !=(FavouriteEntry? left, FavouriteEntry? right) => !Equals(left, right);
	}
}