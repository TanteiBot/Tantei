// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests;

internal sealed class FakeMyAnimeListEnrichmentClient : IMyAnimeListEnrichment
{
	public MediaInfo AnimeDetailsResult { get; init; } = MediaInfo.Empty;

	public MediaInfo MangaDetailsResult { get; init; } = MediaInfo.Empty;

	public IReadOnlyList<SeyuInfo> AnimeSeiyuResult { get; init; } = [];

	public OperationCanceledException? AnimeDetailsCancellation { get; init; }

	public OperationCanceledException? AnimeSeiyuCancellation { get; init; }

	public List<(long Id, CancellationToken CancellationToken)> AnimeDetailsCalls { get; } = [];

	public List<(long Id, CancellationToken CancellationToken)> MangaDetailsCalls { get; } = [];

	public List<(long Id, CancellationToken CancellationToken)> AnimeSeiyuCalls { get; } = [];

	public Task<MediaInfo> GetAnimeDetailsAsync(long id, CancellationToken cancellationToken)
	{
		this.AnimeDetailsCalls.Add((id, cancellationToken));
		return this.AnimeDetailsCancellation is null
			? Task.FromResult(this.AnimeDetailsResult)
			: Task.FromException<MediaInfo>(this.AnimeDetailsCancellation);
	}

	public Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken)
	{
		this.MangaDetailsCalls.Add((id, cancellationToken));
		return Task.FromResult(this.MangaDetailsResult);
	}

	public Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken)
	{
		this.AnimeSeiyuCalls.Add((id, cancellationToken));
		return this.AnimeSeiyuCancellation is null
			? Task.FromResult(this.AnimeSeiyuResult)
			: Task.FromException<IReadOnlyList<SeyuInfo>>(this.AnimeSeiyuCancellation);
	}
}
