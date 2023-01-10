﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;

namespace PaperMalKing.Database.Models.MyAnimeList;

public sealed class MalFavoriteAnime : BaseMalListFavorite, IEquatable<MalFavoriteAnime>
{
	public bool Equals(MalFavoriteAnime? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return this.UserId == other.UserId && this.Id == other.Id;
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is MalFavoriteAnime other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(this.UserId, this.Id);

	public static bool operator ==(MalFavoriteAnime? left, MalFavoriteAnime? right) => Equals(left, right);

	public static bool operator !=(MalFavoriteAnime? left, MalFavoriteAnime? right) => !Equals(left, right);
}