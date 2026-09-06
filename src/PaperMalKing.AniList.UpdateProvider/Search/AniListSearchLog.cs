// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static partial class AniListSearchLog
{
	[LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Fetching seyu detail for media {MediaId} failed, degrading to the snapshot embed")]
	public static partial void SeyuDetailFetchFailed(this ILogger logger, uint mediaId, Exception exception);
}
