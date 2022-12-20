// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.Base;

internal abstract class BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus> where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
																							where TStatus : BaseListEntryStatus<TListStatus>
																							where TMediaType : unmanaged, Enum
																							where TNodeStatus : unmanaged, Enum
																							where TListStatus : unmanaged, Enum
{
	[JsonPropertyName("node")]
	public required TNode Node { get; init; }

	[JsonPropertyName("list_status")]
	public required TStatus Status { get; init; }
}