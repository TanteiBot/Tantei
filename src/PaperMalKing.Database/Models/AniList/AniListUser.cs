// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.AniList;

public sealed class AniListUser : IUpdateProviderUser<AniListUserFeatures>
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public uint Id { get; init; }

	public long LastActivityTimestamp { get; set; }

	public long LastReviewTimestamp { get; set; }

	public ulong DiscordUserId { get; init; }

	public string FavouritesIdHash { get; set; } = null!;

	public AniListUserFeatures Features { get; set; }

	public required DiscordUser DiscordUser { get; init; }

	public required IList<AniListFavourite> Favourites { get; set; }
}