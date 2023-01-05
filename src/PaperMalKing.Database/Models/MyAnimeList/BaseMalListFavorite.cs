// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList;

public abstract class BaseMalListFavorite : BaseMalFavorite
{
	[DatabaseGenerated(DatabaseGeneratedOption.None),Required]
	public required string Type { get; init; }

	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public required ushort StartYear { get; init; }
}