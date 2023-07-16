// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.AniList;

public sealed class AniListFavourite
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public FavouriteType FavouriteType { get; init; }

	public uint UserId { get; set; }

	public AniListUser User { get; set; } = null!;
}