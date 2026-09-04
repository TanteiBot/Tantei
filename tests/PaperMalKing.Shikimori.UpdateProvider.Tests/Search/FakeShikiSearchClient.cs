// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Common.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests.Search;

internal sealed class FakeShikiSearchClient : IShikiClient
{
	public IReadOnlyList<AnimeSearchMedia> AnimeResults { get; init; } = [];

	public IReadOnlyList<MangaSearchMedia> MangaResults { get; init; } = [];

	public Exception? SearchException { get; init; }

	public int CallCount { get; private set; }

	public List<string> Queries { get; } = [];

	public List<AnimeKind?> AnimeKinds { get; } = [];

	public List<MangaKind?> MangaKinds { get; } = [];

	public List<bool> IncludeNsfws { get; } = [];

	public Task<IReadOnlyList<AnimeSearchMedia>> SearchAnimeAsync(string query, AnimeKind? kind, bool includeNsfw, CancellationToken cancellationToken)
	{
		this.CallCount++;
		this.Queries.Add(query);
		this.AnimeKinds.Add(kind);
		this.IncludeNsfws.Add(includeNsfw);
		return this.SearchException is null
			? Task.FromResult(this.AnimeResults)
			: Task.FromException<IReadOnlyList<AnimeSearchMedia>>(this.SearchException);
	}

	public Task<IReadOnlyList<MangaSearchMedia>> SearchMangaAsync(string query, MangaKind? kind, bool includeNsfw, CancellationToken cancellationToken)
	{
		this.CallCount++;
		this.Queries.Add(query);
		this.MangaKinds.Add(kind);
		this.IncludeNsfws.Add(includeNsfw);
		return this.SearchException is null
			? Task.FromResult(this.MangaResults)
			: Task.FromException<IReadOnlyList<MangaSearchMedia>>(this.SearchException);
	}

	public Task<UserInfo> GetUserByNicknameAsync(string nickname, CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<UserInfo> GetUserByIdAsync(uint userId, CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<Favourites> GetUserFavouritesAsync(uint userId, CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<Paginatable<History>> GetUserHistoryAsync(uint userId, uint page, byte limit, HistoryRequestOptions options, CancellationToken cancellationToken) =>
		throw new NotSupportedException();

	public Task<TMedia?> GetMediaAsync<TMedia>(ulong id, ListEntryType type, RequestOptions options, CancellationToken cancellationToken)
		where TMedia : BaseMedia => throw new NotSupportedException();

	public Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(uint userId, CancellationToken cancellationToken) => throw new NotSupportedException();

	public Task<byte[]?> GetImageContentAsync(string url, CancellationToken cancellationToken) => throw new NotSupportedException();
}
