// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

namespace PaperMalKing.MyAnimeList.Wrapper;

internal sealed class ListQueryResult<T, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
	where T : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
	where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
	where TStatus : BaseListEntryStatus<TListStatus>
	where TMediaType : unmanaged, Enum
	where TNodeStatus : unmanaged, Enum
	where TListStatus : unmanaged, Enum
{
	public static ListQueryResult<T, TNode, TStatus, TMediaType, TNodeStatus, TListStatus> Empty { get; } = new()
	{
		Data = [],
	};

	[JsonPropertyName("data")]
	public required T[] Data { get; init; }
}