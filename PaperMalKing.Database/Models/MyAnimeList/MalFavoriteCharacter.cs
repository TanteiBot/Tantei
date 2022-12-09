// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList;

public sealed class MalFavoriteCharacter : IMalFavorite, IEquatable<MalFavoriteCharacter>
{
	public string FromTitleName { get; init; } = null!;

	[Obsolete("Mal no longer provides url for favorite characters", true)]
	public string FromTitleUrl { get; init; } = "";

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
	public bool Equals(MalFavoriteCharacter? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return this.UserId == other.UserId && this.Id == other.Id;
	}

	/// <inheritdoc />
	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is MalFavoriteCharacter other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			return (this.UserId * 397) ^ this.Id;
		}
	}

	public static bool operator ==(MalFavoriteCharacter? left, MalFavoriteCharacter? right) => Equals(left, right);

	public static bool operator !=(MalFavoriteCharacter? left, MalFavoriteCharacter? right) => !Equals(left, right);
}