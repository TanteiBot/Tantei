// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Common.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests;

internal sealed class FakeShikiClient : IShikiClient
{
	public Favourites Favourites { get; init; } = Favourites.Empty;

	public FakeShikiFavouriteClient Favourite { get; init; } = new();

	public Task<Favourites> GetUserFavouritesAsync(uint userId, CancellationToken cancellationToken) => Task.FromResult(this.Favourites);

	public Task<FavouritesInfo> GetFavouritesInfoAsync(FavouriteIds ids, RequestOptions options, CancellationToken cancellationToken) =>
		this.Favourite.GetFavouritesInfoAsync(ids, options, cancellationToken);

	public Task<CharacterDetails?> GetCharacterDetailsAsync(uint id, CancellationToken cancellationToken) =>
		this.Favourite.GetCharacterDetailsAsync(id, cancellationToken);

	public Task<PersonDetails?> GetPersonDetailsAsync(uint id, CancellationToken cancellationToken) =>
		this.Favourite.GetPersonDetailsAsync(id, cancellationToken);

	public Task<UserInfo> GetUserByIdAsync(uint userId, CancellationToken cancellationToken) =>
		Task.FromResult(new UserInfo { Id = userId, Nickname = "test-user", });

	public Task<Paginatable<History>> GetUserHistoryAsync(uint userId, uint page, byte limit, HistoryRequestOptions options,
														  CancellationToken cancellationToken) => Task.FromResult(Paginatable<History>.Empty);

	public Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(uint userId, CancellationToken cancellationToken) =>
		Task.FromResult<IReadOnlyList<UserAchievement>>([]);

	public Task<byte[]?> GetImageContentAsync(string url, CancellationToken cancellationToken) => Task.FromResult<byte[]?>(null);

	public Task<UserInfo> GetUserByNicknameAsync(string nickname, CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<TMedia?> GetMediaAsync<TMedia>(ulong id, ListEntryType type, RequestOptions options, CancellationToken cancellationToken)
		where TMedia : BaseMedia => throw new NotSupportedException();

	public Task<IReadOnlyList<AnimeSearchMedia>> SearchAnimeAsync(string query, AnimeKind? kind, bool includeNsfw,
																   CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<IReadOnlyList<MangaSearchMedia>> SearchMangaAsync(string query, MangaKind? kind, bool includeNsfw,
																   CancellationToken cancellationToken) => throw new NotSupportedException();
}
