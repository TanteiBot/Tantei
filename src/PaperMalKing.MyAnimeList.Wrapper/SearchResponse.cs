// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper;

internal sealed class SearchResponse<TResult>
{
	[JsonPropertyName("data")]
	public required IReadOnlyList<ResultEnvelope> Results { get; init; }

	public sealed class ResultEnvelope
	{
		[JsonPropertyName("node")]
		public required TResult Result { get; init; }
	}
}
