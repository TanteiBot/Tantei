// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;
using PaperMalKing.MyAnimeList.Wrapper.Models.Status;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List
{
	internal interface IListEntry
	{
		bool IsReprogressing { get; }
		int Id { get; init; }
		GenericProgress UserProgress { get; }
		string Title { get; }
		int Score { get; }
		string Tags { get; }
		int ProgressedSubEntries { get; }
		int TotalSubEntries { get; }
		GenericStatus Status { get; }
		string Url { get; init; }
		string ImageUrl { get; init; }
		string MediaType { get; init; }
	}
}