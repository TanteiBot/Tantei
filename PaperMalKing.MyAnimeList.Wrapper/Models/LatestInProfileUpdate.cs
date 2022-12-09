// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

namespace PaperMalKing.MyAnimeList.Wrapper.Models;

internal class LatestInProfileUpdate
{
	private (string inRssHash, string inProfileHash)? _hash = null;
	internal int Id { get; init; }

	internal GenericProgress Progress { get; init; }

	internal int ProgressValue { get; init; }

	internal int Score { get; init; }

	internal (string inRssHash, string inProfileHash) Hash => this._hash ??= Extensions.GetHash(this.Id, this.ProgressValue, this.Progress, this.Score);
}