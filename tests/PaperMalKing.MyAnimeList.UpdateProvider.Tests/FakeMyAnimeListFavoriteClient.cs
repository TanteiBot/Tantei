// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests;

internal sealed class FakeMyAnimeListFavoriteClient : IMyAnimeListClient
{
	public AnimeSearchResult? AnimeResult { get; init; }

	public MangaSearchResult? MangaResult { get; init; }

	public Exception? OfficialException { get; init; }

	public MediaInfo AnimeDetailsResult { get; init; } = MediaInfo.Empty;

	public MediaInfo MangaDetailsResult { get; init; } = MediaInfo.Empty;

	public IReadOnlyList<SeyuInfo> AnimeSeiyuResult { get; init; } = [];

	public EntityInfo CharacterInfoResult { get; init; } = EntityInfo.Empty;

	public EntityInfo PersonInfoResult { get; init; } = EntityInfo.Empty;

	public EntityInfo StudioInfoResult { get; init; } = EntityInfo.Empty;

	public List<uint> AnimeByIdCalls { get; } = [];

	public List<uint> MangaByIdCalls { get; } = [];

	public List<long> AnimeDetailsCalls { get; } = [];

	public List<long> MangaDetailsCalls { get; } = [];

	public List<long> AnimeSeiyuCalls { get; } = [];

	public List<(long Id, bool WithDescription)> CharacterInfoCalls { get; } = [];

	public List<(long Id, bool WithDescription)> PersonInfoCalls { get; } = [];

	public List<long> StudioInfoCalls { get; } = [];

	public int TotalCallCount => this.AnimeByIdCalls.Count + this.MangaByIdCalls.Count + this.AnimeDetailsCalls.Count +
		this.MangaDetailsCalls.Count + this.AnimeSeiyuCalls.Count + this.CharacterInfoCalls.Count + this.PersonInfoCalls.Count +
		this.StudioInfoCalls.Count;

	public Task<AnimeSearchResult?> GetAnimeByIdAsync(uint id, CancellationToken cancellationToken)
	{
		this.AnimeByIdCalls.Add(id);
		return this.OfficialException is null ? Task.FromResult(this.AnimeResult) : Task.FromException<AnimeSearchResult?>(this.OfficialException);
	}

	public Task<MangaSearchResult?> GetMangaByIdAsync(uint id, CancellationToken cancellationToken)
	{
		this.MangaByIdCalls.Add(id);
		return this.OfficialException is null ? Task.FromResult(this.MangaResult) : Task.FromException<MangaSearchResult?>(this.OfficialException);
	}

	public Task<MediaInfo> GetAnimeDetailsAsync(long id, CancellationToken cancellationToken)
	{
		this.AnimeDetailsCalls.Add(id);
		return Task.FromResult(this.AnimeDetailsResult);
	}

	public Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken)
	{
		this.MangaDetailsCalls.Add(id);
		return Task.FromResult(this.MangaDetailsResult);
	}

	public Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken)
	{
		this.AnimeSeiyuCalls.Add(id);
		return Task.FromResult(this.AnimeSeiyuResult);
	}

	public Task<EntityInfo> GetCharacterInfoAsync(long id, bool withDescription, CancellationToken cancellationToken)
	{
		this.CharacterInfoCalls.Add((id, withDescription));
		return Task.FromResult(this.CharacterInfoResult);
	}

	public Task<EntityInfo> GetPersonInfoAsync(long id, bool withDescription, CancellationToken cancellationToken)
	{
		this.PersonInfoCalls.Add((id, withDescription));
		return Task.FromResult(this.PersonInfoResult);
	}

	public Task<EntityInfo> GetStudioInfoAsync(long id, CancellationToken cancellationToken)
	{
		this.StudioInfoCalls.Add(id);
		return Task.FromResult(this.StudioInfoResult);
	}

	public Task<User> GetUserAsync(string username, ParserOptions options, CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<string> GetUsernameAsync(uint id, CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<IReadOnlyList<TE>> GetLatestListUpdatesAsync<TE, TListType, TRequestOptions, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
		string username, TRequestOptions requestOptions, CancellationToken cancellationToken)
		where TE : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
		where TListType : IListType
		where TRequestOptions : unmanaged, Enum
		where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
		where TStatus : BaseListEntryStatus<TListStatus>
		where TMediaType : unmanaged, Enum
		where TNodeStatus : unmanaged, Enum
		where TListStatus : unmanaged, Enum => throw new NotSupportedException();

	public Task<IReadOnlyList<AnimeSearchResult>> SearchAnimeAsync(string query, bool includeNsfw, CancellationToken cancellationToken) =>
		throw new NotSupportedException();

	public Task<IReadOnlyList<MangaSearchResult>> SearchMangaAsync(string query, bool includeNsfw, CancellationToken cancellationToken) =>
		throw new NotSupportedException();
}
