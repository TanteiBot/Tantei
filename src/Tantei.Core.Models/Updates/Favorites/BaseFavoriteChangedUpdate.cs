// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Updates.Favorites;

public abstract record BaseFavoriteChangedUpdate<T>(Author Author, ProviderInfo Provider, T Value, FavoriteStatus Status,
													DateTimeOffset? TimeStamp = null) : BaseUserUpdate(Author, Provider, TimeStamp,
	Value is IHasImage hi ? hi.ImageUrl : null) where T : class, INameUrlable;