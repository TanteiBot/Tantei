// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed class FavouriteMediaRoles
{
	public required FavouriteEntry FavouriteEntry { get; init; }

	public BaseMedia? Media { get; set; }

	public IReadOnlyList<Role>? Roles { get; set; }
}