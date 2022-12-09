// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

namespace PaperMalKing.MyAnimeList.Wrapper.Models;

internal sealed class RecentUpdate
{
	private (string inRssHash, string inProfileHash)? _hash = null;
	internal ListEntryType ListType { get; init; }

	internal int Id { get; init; }

	internal DateTimeOffset UpdateDateTime { get; init; }

	internal GenericProgress ProgressValue { get; init; }

	internal int ProgressedEntries { get; init; }

	internal (string inRssHash, string inProfileHash) Hash => this._hash ??= Extensions.GetHash(this.Id, this.ProgressedEntries, this.ProgressValue, 0);

	internal RecentUpdate(ListEntryType listType, int id, DateTimeOffset updateDateTime, GenericProgress progressValue, int progressedEntries)
	{
		this.ListType = listType;
		this.Id = id;
		this.UpdateDateTime = updateDateTime;
		this.ProgressValue = progressValue;
		this.ProgressedEntries = progressedEntries;
	}
}