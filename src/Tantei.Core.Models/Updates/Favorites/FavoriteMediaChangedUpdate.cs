// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Tantei.Core.Models.Medias;

namespace Tantei.Core.Models.Updates.Favorites;

public sealed record FavoriteMediaChangedUpdate(Author Author, ProviderInfo Provider, Media Value, FavoriteStatus Status,
												DateTimeOffset? TimeStamp = null) : BaseFavoriteChangedUpdate<Media>(Author, Provider, Value, Status,
	TimeStamp);