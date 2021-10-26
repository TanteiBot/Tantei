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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	public sealed class MalFavoriteManga : IMalListFavorite, IEquatable<MalFavoriteManga>
	{
		[ForeignKey(nameof(User))]
		public int UserId { get; init; }

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int Id { get; init; }

		/// <inheritdoc />
		public string? ImageUrl { get; init; }

		/// <inheritdoc />
		public string Name { get; init; } = null!;

		/// <inheritdoc />
		public string NameUrl { get; init; } = null!;

		/// <inheritdoc />
		public MalUser User { get; init; } = null!;

		/// <inheritdoc />
		public string Type { get; init; } = null!;

		/// <inheritdoc />
		public int StartYear { get; init; }

		/// <inheritdoc />
		public bool Equals(MalFavoriteManga? other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return this.UserId == other.UserId && this.Id == other.Id;
		}

		/// <inheritdoc />
		public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is MalFavoriteManga other && Equals(other);

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return (this.UserId * 397) ^ this.Id;
			}
		}

		public static bool operator ==(MalFavoriteManga? left, MalFavoriteManga? right) => Equals(left, right);

		public static bool operator !=(MalFavoriteManga? left, MalFavoriteManga? right) => !Equals(left, right);
	}
}