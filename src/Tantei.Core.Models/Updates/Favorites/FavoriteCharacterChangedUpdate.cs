// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Updates.Favorites;

public sealed record FavoriteCharacterChangedUpdate(Author Author, ProviderInfo Provider, Character Value, FavoriteStatus Status,
													DateTimeOffset? TimeStamp = null) : BaseFavoriteChangedUpdate<Character>(Author, Provider, Value,
	Status, TimeStamp);