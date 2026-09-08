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

	public FavouritesInfo FavouritesInfo { get; init; } = FavouritesInfo.Empty;

	public CharacterDetails? CharacterDetails { get; init; }

	public PersonDetails? PersonDetails { get; init; }

	public Exception? CharacterDetailsException { get; init; }

	public Exception? PersonDetailsException { get; init; }

	public List<FavouriteIds> FavouritesInfoCalls { get; } = [];

	public List<uint> CharacterDetailsCalls { get; } = [];

	public List<uint> PersonDetailsCalls { get; } = [];

	public Task<Favourites> GetUserFavouritesAsync(uint userId, CancellationToken cancellationToken) => Task.FromResult(this.Favourites);

	public Task<FavouritesInfo> GetFavouritesInfoAsync(FavouriteIds ids, RequestOptions options, CancellationToken cancellationToken)
	{
		this.FavouritesInfoCalls.Add(ids);
		return Task.FromResult(this.FavouritesInfo);
	}

	public Task<CharacterDetails?> GetCharacterDetailsAsync(uint id, CancellationToken cancellationToken)
	{
		this.CharacterDetailsCalls.Add(id);
		return this.CharacterDetailsException is null
			? Task.FromResult(this.CharacterDetails)
			: Task.FromException<CharacterDetails?>(this.CharacterDetailsException);
	}

	public Task<PersonDetails?> GetPersonDetailsAsync(uint id, CancellationToken cancellationToken)
	{
		this.PersonDetailsCalls.Add(id);
		return this.PersonDetailsException is null
			? Task.FromResult(this.PersonDetails)
			: Task.FromException<PersonDetails?>(this.PersonDetailsException);
	}

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
