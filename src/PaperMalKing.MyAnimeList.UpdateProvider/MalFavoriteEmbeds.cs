// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal static class MalFavoriteEmbeds
{
	public static async Task<IReadOnlyList<DiscordEmbedBuilder>> BuildAsync(
		IReadOnlyList<EnrichedMalFavorite> favorites,
		User user,
		MalUser dbUser,
		IMyAnimeListClient client,
		ILogger<BaseUpdateProvider> logger,
		CancellationToken cancellationToken)
	{
		foreach (var favorite in favorites)
		{
			await EnrichAsync(favorite, dbUser.Features, client, logger, cancellationToken);
		}

		var result = new List<DiscordEmbedBuilder>(favorites.Count);
		foreach (var favorite in favorites)
		{
			var eb = ToDiscordEmbedBuilder(favorite, dbUser);
			eb.WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl);
			result.Add(eb);
		}

		return result.SortByThenBy(static f => f.Color.HasValue ? f.Color.Value.Value : DiscordColor.None.Value, static f => f.Title);
	}

	private static async Task EnrichAsync(
		EnrichedMalFavorite favorite,
		MalUserFeatures features,
		IMyAnimeListClient client,
		ILogger<BaseUpdateProvider> logger,
		CancellationToken cancellationToken)
	{
		var id = favorite.Favorite.Id;
		var withDescription = features.HasFlag(MalUserFeatures.Synopsis);
		switch (favorite.Favorite)
		{
			case MalFavoriteAnime:
				favorite.Anime = await TryAsync(token => client.GetAnimeByIdAsync(id, token), fallback: null, favorite, logger, cancellationToken);
				if (features.HasAnyFlag(MalUserFeatures.Themes, MalUserFeatures.Demographic))
				{
					favorite.MediaInfo = await TryAsync(token => client.GetAnimeDetailsAsync(id, token), MediaInfo.Empty, favorite, logger,
						cancellationToken);
				}

				if (features.HasFlag(MalUserFeatures.Seiyu))
				{
					favorite.Seiyu = await TryAsync(token => client.GetAnimeSeiyuAsync(id, token), [], favorite, logger, cancellationToken);
				}

				break;
			case MalFavoriteManga:
				favorite.Manga = await TryAsync(token => client.GetMangaByIdAsync(id, token), fallback: null, favorite, logger, cancellationToken);
				if (features.HasAnyFlag(MalUserFeatures.Themes, MalUserFeatures.Demographic))
				{
					favorite.MediaInfo = await TryAsync(token => client.GetMangaDetailsAsync(id, token), MediaInfo.Empty, favorite, logger,
						cancellationToken);
				}

				break;
			case MalFavoriteCharacter:
				favorite.Entity = await TryAsync(token => client.GetCharacterInfoAsync(id, withDescription, token), EntityInfo.Empty, favorite,
					logger, cancellationToken);
				break;
			case MalFavoritePerson:
				favorite.Entity = await TryAsync(token => client.GetPersonInfoAsync(id, withDescription, token), EntityInfo.Empty, favorite, logger,
					cancellationToken);
				break;
			case MalFavoriteCompany:
				favorite.Entity = await TryAsync(token => client.GetStudioInfoAsync(id, token), EntityInfo.Empty, favorite, logger, cancellationToken);
				break;
			default:
				throw new UnreachableException();
		}
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Enrichment is best effort")]
	private static async Task<TValue> TryAsync<TValue>(
		Func<CancellationToken, Task<TValue>> request,
		TValue fallback,
		EnrichedMalFavorite favorite,
		ILogger<BaseUpdateProvider> logger,
		CancellationToken cancellationToken)
	{
		try
		{
			return await request(cancellationToken);
		}
		catch (Exception exception) when (exception is not OperationCanceledException)
		{
			logger.FailedToEnrichFavorite(exception, favorite.Favorite.FavoriteType, favorite.Favorite.Id);
			return fallback;
		}
	}

	private static DiscordEmbedBuilder ToDiscordEmbedBuilder(EnrichedMalFavorite enriched, MalUser dbUser)
	{
		var favorite = enriched.Favorite;
		var added = enriched.Added;
		var features = dbUser.Features;
		var color = dbUser.Colors.Find(added
			? static c => c.UpdateType == (byte)MalUpdateType.FavoriteAdded
			: static c => c.UpdateType == (byte)MalUpdateType.FavoriteRemoved)?.ColorValue ?? (added ? Constants.MalGreen : Constants.MalRed);

		var eb = new DiscordEmbedBuilder
		{
			Url = favorite.NameUrl,
		}.WithDescription($"{(added ? "Added" : "Removed")} favorite").WithColor(color);

		switch (favorite)
		{
			case BaseMalListFavorite listFavorite:
				FillMedia(eb, listFavorite, enriched, features);
				break;
			case MalFavoriteCharacter character:
				MalMediaEmbeds.AddThumbnail(eb, favorite.ImageUrl);
				eb.WithTitle($"{character.Name} [Character]");
				MalMediaEmbeds.AddDescription(eb, enriched.Entity.Description, features);
				AddBestKnownWork(eb, "From", enriched.Entity.BestKnownWork, character.FromTitleName);
				break;
			case MalFavoritePerson person:
				MalMediaEmbeds.AddThumbnail(eb, favorite.ImageUrl);
				eb.WithTitle($"{person.Name} [Person]");
				MalMediaEmbeds.AddDescription(eb, enriched.Entity.Description, features);
				AddBestKnownWork(eb, "From", enriched.Entity.BestKnownWork, fallback: null);
				break;
			case MalFavoriteCompany company:
				MalMediaEmbeds.AddThumbnail(eb, favorite.ImageUrl);
				eb.WithTitle($"{company.Name} [Studio]");
				AddBestKnownWork(eb, "Known for", enriched.Entity.BestKnownWork, fallback: null);
				break;
			default:
				throw new UnreachableException();
		}

		return eb;
	}

	private static void FillMedia(DiscordEmbedBuilder eb, BaseMalListFavorite favorite, EnrichedMalFavorite enriched, MalUserFeatures features)
	{
		var anime = enriched.Anime;
		var manga = enriched.Manga;
		var picture = anime?.Picture ?? manga?.Picture;
		MalMediaEmbeds.AddThumbnail(eb, picture, favorite.ImageUrl);

		var format = features.HasFlag(MalUserFeatures.MediaFormat) ? $" ({favorite.Type})" : "";
		var title = anime?.PrimaryTitle ?? manga?.PrimaryTitle ?? favorite.Name;
		eb.WithTitle($"{title}{format} [{favorite.StartYear.ToString(CultureInfo.InvariantCulture)}]");

		var result = (BaseSearchResult?)anime ?? manga;
		MalMediaEmbeds.AddStatus(eb, result, features);
		MalMediaEmbeds.AddScore(eb, result);
		MalMediaEmbeds.AddTotal(eb, result);

		MalMediaEmbeds.AddGenres(eb, result?.Genres, features);
		MalMediaEmbeds.AddStudios(eb, anime?.Studios, features);
		MalMediaEmbeds.AddAuthors(eb, manga?.Authors, features);
		MalMediaEmbeds.AddThemes(eb, enriched.MediaInfo, features);
		MalMediaEmbeds.AddDemographic(eb, enriched.MediaInfo, features);
		MalMediaEmbeds.AddSeiyu(eb, enriched.Seiyu, features);
		MalMediaEmbeds.AddSynopsis(eb, result?.Synopsis, features);
	}

	private static void AddBestKnownWork(DiscordEmbedBuilder eb, string fieldName, BestKnownWork? work, string? fallback)
	{
		if (work is null)
		{
			eb.AddFieldIfPresent(fieldName, fallback, inline: true);
			return;
		}

		eb.AddField(fieldName, Formatter.MaskedUrl(work.Title, new(work.Url)), inline: true);
	}
}
