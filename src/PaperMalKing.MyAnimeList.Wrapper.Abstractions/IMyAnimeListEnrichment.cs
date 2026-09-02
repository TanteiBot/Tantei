// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions;

public interface IMyAnimeListEnrichment
{
	Task<MediaInfo> GetAnimeDetailsAsync(long id, CancellationToken cancellationToken);

	Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken);

	Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken);
}
