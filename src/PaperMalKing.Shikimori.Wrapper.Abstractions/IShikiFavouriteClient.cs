// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions;

public interface IShikiFavouriteClient
{
	Task<FavouritesInfo> GetFavouritesInfoAsync(FavouriteIds ids, RequestOptions options, CancellationToken cancellationToken);

	Task<CharacterDetails?> GetCharacterDetailsAsync(uint id, CancellationToken cancellationToken);

	Task<PersonDetails?> GetPersonDetailsAsync(uint id, CancellationToken cancellationToken);
}
