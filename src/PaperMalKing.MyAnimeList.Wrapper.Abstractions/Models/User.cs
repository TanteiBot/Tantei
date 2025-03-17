// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;

public sealed class User
{
	public required string Username { get; init; }

	[field: MaybeNull]
	[field: AllowNull]
	public string ProfileUrl => field ??= Constants.ProfileUrl + this.Username;

	[field: MaybeNull]
	[field: AllowNull]
	public string AvatarUrl => field ??=
			string.Create(CultureInfo.InvariantCulture, $"{Constants.UserAvatar}{this.Id}.jpg?t={TimeProvider.System.GetUtcNow().ToUnixTimeSeconds()}");

	public uint Id { get; init; }

	public bool HasPublicAnimeUpdates => this.LatestAnimeUpdateHash is not null;

	public bool HasPublicMangaUpdates => this.LatestMangaUpdateHash is not null;

	public string? LatestMangaUpdateHash { get; init; }

	public string? LatestAnimeUpdateHash { get; init; }

	public required UserFavorites Favorites { get; init; }
}