// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

internal sealed class FakeMyAnimeListSearchClient : IMyAnimeListClient
{
	public AnimeSearchResponse AnimeResponse { get; init; } = AnimeSearchResponse.Empty;

	public MangaSearchResponse MangaResponse { get; init; } = MangaSearchResponse.Empty;

	public Exception? SearchException { get; init; }

	public int CallCount { get; private set; }

	public List<string> Queries { get; } = [];

	public List<bool> NsfwFlags { get; } = [];

	public Task<AnimeSearchResponse> SearchAnimeAsync(string query, bool includeNsfw, CancellationToken cancellationToken)
	{
		this.Record(query, includeNsfw);
		return this.SearchException is null ? Task.FromResult(this.AnimeResponse) : Task.FromException<AnimeSearchResponse>(this.SearchException);
	}

	public Task<MangaSearchResponse> SearchMangaAsync(string query, bool includeNsfw, CancellationToken cancellationToken)
	{
		this.Record(query, includeNsfw);
		return this.SearchException is null ? Task.FromResult(this.MangaResponse) : Task.FromException<MangaSearchResponse>(this.SearchException);
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

	public Task<MediaInfo> GetAnimeDetailsAsync(long id, CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken) => throw new NotSupportedException();

	private void Record(string query, bool includeNsfw)
	{
		this.CallCount++;
		this.Queries.Add(query);
		this.NsfwFlags.Add(includeNsfw);
	}
}
