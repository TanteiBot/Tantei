// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions;

public interface IMyAnimeListClient
{
	Task<User> GetUserAsync(string username, ParserOptions options, CancellationToken cancellationToken = default);

	Task<string> GetUsernameAsync(uint id, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<TE>>
		GetLatestListUpdatesAsync<TE, TListType, TRequestOptions, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
			string username, TRequestOptions requestOptions, CancellationToken cancellationToken = default)
		where TE : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
		where TListType : IListType
		where TRequestOptions : unmanaged, Enum
		where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
		where TStatus : BaseListEntryStatus<TListStatus>
		where TMediaType : unmanaged, Enum
		where TNodeStatus : unmanaged, Enum
		where TListStatus : unmanaged, Enum;

	Task<MediaInfo> GetAnimeDetailsAsync(long id,  CancellationToken cancellationToken = default);

	Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken = default);
}