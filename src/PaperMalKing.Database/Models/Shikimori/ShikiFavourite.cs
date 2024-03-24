// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.Shikimori;

public sealed class ShikiFavourite
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public required string FavType { get; init; }

	public required string Name { get; init; }

	public uint UserId { get; init; }

	public ShikiUser User { get; set; } = null!;
}