// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;

public sealed class User
{
	private string? _avatarUrl;
	private string? _profileUrl;
	public required string Username { get; init; }

	public string ProfileUrl => this._profileUrl ??= (Constants.PROFILE_URL + this.Username);

	public string AvatarUrl =>
		this._avatarUrl ??=
			$"{Constants.USER_AVATAR}{this.Id}.jpg?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

	public uint Id { get; init; }

	public bool HasPublicAnimeUpdates => this.LatestAnimeUpdateHash is not null;

	public bool HasPublicMangaUpdates => this.LatestMangaUpdateHash is not null;

	public string? LatestMangaUpdateHash { get; init; }

	public string? LatestAnimeUpdateHash { get; init; }

	public required UserFavorites Favorites { get; init; }
}