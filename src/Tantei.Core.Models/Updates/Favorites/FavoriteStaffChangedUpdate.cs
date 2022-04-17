// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Tantei.Core.Models.Medias;

namespace Tantei.Core.Models.Updates.Favorites;

public sealed record FavoriteStaffChangedUpdate(Author Author, ProviderInfo Provider, Staff Value, FavoriteStatus Status,
												DateTimeOffset? TimeStamp = null) : BaseFavoriteChangedUpdate<Staff>(Author, Provider, Value, Status,
	TimeStamp);