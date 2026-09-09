// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests;

internal sealed class FakeShikiFavouriteClient : IShikiFavouriteClient
{
	public FavouritesInfo FavouritesInfo { get; init; } = FavouritesInfo.Empty;

	public CharacterDetails? CharacterDetails { get; init; }

	public PersonDetails? PersonDetails { get; init; }

	public Exception? CharacterDetailsException { get; init; }

	public Exception? PersonDetailsException { get; init; }

	public List<FavouriteIds> FavouritesInfoCalls { get; } = [];

	public List<uint> CharacterDetailsCalls { get; } = [];

	public List<uint> PersonDetailsCalls { get; } = [];

	public int EnrichmentCallCount => this.CharacterDetailsCalls.Count + this.PersonDetailsCalls.Count;

	public Task<FavouritesInfo> GetFavouritesInfoAsync(FavouriteIds ids, RequestOptions options, CancellationToken cancellationToken)
	{
		this.FavouritesInfoCalls.Add(ids);
		return Task.FromResult(this.FavouritesInfo);
	}

	public Task<CharacterDetails?> GetCharacterDetailsAsync(uint id, CancellationToken cancellationToken)
	{
		this.CharacterDetailsCalls.Add(id);
		return this.CharacterDetailsException is null
			? Task.FromResult(this.CharacterDetails)
			: Task.FromException<CharacterDetails?>(this.CharacterDetailsException);
	}

	public Task<PersonDetails?> GetPersonDetailsAsync(uint id, CancellationToken cancellationToken)
	{
		this.PersonDetailsCalls.Add(id);
		return this.PersonDetailsException is null
			? Task.FromResult(this.PersonDetails)
			: Task.FromException<PersonDetails?>(this.PersonDetailsException);
	}
}
