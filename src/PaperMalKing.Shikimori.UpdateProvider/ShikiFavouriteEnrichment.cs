// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal static class ShikiFavouriteEnrichment
{
	public static async Task<(IReadOnlyList<EnrichedFavourite> AddedValues, IReadOnlyList<EnrichedFavourite> RemovedValues)> EnrichAsync(
		IReadOnlyList<FavouriteEntry> addedValues,
		IReadOnlyList<FavouriteEntry> removedValues,
		ShikiUserFeatures features,
		IShikiFavouriteClient client,
		ILogger<ShikiUpdateProvider> logger,
		CancellationToken cancellationToken)
	{
		var addedFavourites = addedValues.Select(static x => new EnrichedFavourite { FavouriteEntry = x }).ToArray();
		var removedFavourites = removedValues.Select(static x => new EnrichedFavourite { FavouriteEntry = x }).ToArray();

		await EnrichAsync([.. addedFavourites, .. removedFavourites], features, client, logger, cancellationToken);

		return (addedFavourites, removedFavourites);
	}

	private static async Task EnrichAsync(IReadOnlyList<EnrichedFavourite> favourites, ShikiUserFeatures features, IShikiFavouriteClient client,
										  ILogger<ShikiUpdateProvider> logger, CancellationToken cancellationToken)
	{
		if (favourites is [])
		{
			return;
		}

		var info = await client.GetFavouritesInfoAsync(new()
		{
			AnimeIds = IdsOf(favourites, ShikiFavouriteKind.Anime),
			MangaIds = IdsOf(favourites, ShikiFavouriteKind.Manga),
			CharacterIds = IdsOf(favourites, ShikiFavouriteKind.Character),
			PersonIds = IdsOf(favourites, ShikiFavouriteKind.Person),
		}, (RequestOptions)features, cancellationToken);

		foreach (var favourite in favourites)
		{
			var id = favourite.FavouriteEntry.Id;
			favourite.Media = favourite.Kind switch
			{
				ShikiFavouriteKind.Anime => info.Animes.FirstOrDefault(x => x.Id == id),
				ShikiFavouriteKind.Manga => info.Mangas.FirstOrDefault(x => x.Id == id),
				_ => null,
			};
			favourite.Character = favourite.Kind == ShikiFavouriteKind.Character ? info.Characters.FirstOrDefault(x => x.Id == id) : null;
			favourite.Person = favourite.Kind == ShikiFavouriteKind.Person ? info.People.FirstOrDefault(x => x.Id == id) : null;
		}

		foreach (var favourite in favourites)
		{
			await FillBestKnownWorkAsync(favourite, client, logger, cancellationToken);
		}

		static IReadOnlyList<uint> IdsOf(IReadOnlyList<EnrichedFavourite> source, ShikiFavouriteKind kind) =>
			[.. source.Where(x => x.Kind == kind).Select(static x => x.FavouriteEntry.Id).Distinct()];
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Enrichment is best effort")]
	private static async Task FillBestKnownWorkAsync(EnrichedFavourite favourite, IShikiFavouriteClient client, ILogger<ShikiUpdateProvider> logger,
													CancellationToken cancellationToken)
	{
		try
		{
			if (favourite.Character is not null)
			{
				var details = await client.GetCharacterDetailsAsync(favourite.FavouriteEntry.Id, cancellationToken);
				favourite.BestKnownWork = details?.BestKnownWork();
			}

			if (favourite.Person is { } person)
			{
				var details = await client.GetPersonDetailsAsync(favourite.FavouriteEntry.Id, cancellationToken);
				favourite.BestKnownWork = details?.BestKnownWork(person.PrefersRolesOverWorks());
			}
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.FailedToEnrichFavourite(ex, favourite.FavouriteEntry.GenericType ?? "unknown", favourite.FavouriteEntry.Id);
		}
	}
}
