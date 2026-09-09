// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed class EnrichedFavourite
{
	public required FavouriteEntry FavouriteEntry { get; init; }

	public BaseMedia? Media { get; set; }

	public FavouriteCharacter? Character { get; set; }

	public FavouritePerson? Person { get; set; }

	public RelatedMedia? BestKnownWork { get; set; }

	public RelatedCharacter? BestKnownWorkCharacter { get; set; }

	public ShikiFavouriteKind Kind => this.FavouriteEntry.GenericType switch
	{
		"animes" => ShikiFavouriteKind.Anime,
		"mangas" => ShikiFavouriteKind.Manga,
		"characters" => ShikiFavouriteKind.Character,
		"people" => ShikiFavouriteKind.Person,
		_ => ShikiFavouriteKind.Unknown,
	};
}
