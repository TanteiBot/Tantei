// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList;

public sealed class MalFavoritePerson : IMalFavorite, IEquatable<MalFavoritePerson>
{
	[ForeignKey(nameof(User))]
	public int UserId { get; init; }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public int Id { get; init; }

	public string? ImageUrl { get; init; }

	public required string Name { get; init; }

	public required string NameUrl { get; init; }

	public required MalUser User { get; init; }

	public bool Equals(MalFavoritePerson? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return this.UserId == other.UserId && this.Id == other.Id;
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is MalFavoritePerson other && Equals(other);

	public override int GetHashCode()
	{
		unchecked
		{
			return (this.UserId * 397) ^ this.Id;
		}
	}

	public static bool operator ==(MalFavoritePerson? left, MalFavoritePerson? right) => Equals(left, right);

	public static bool operator !=(MalFavoritePerson? left, MalFavoritePerson? right) => !Equals(left, right);
}