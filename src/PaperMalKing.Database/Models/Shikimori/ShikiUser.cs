// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.Shikimori;

public sealed class ShikiUser : IUpdateProviderUser<ShikiUserFeatures>
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	public uint LastHistoryEntryId { get; set; }

	public ulong DiscordUserId { get; init; }

	public ShikiUserFeatures Features { get; set; }

	public required DiscordUser DiscordUser { get; set; }

	public string FavouritesIdHash { get; set; } = null!;

	public required IList<ShikiFavourite> Favourites { get; set; }
}