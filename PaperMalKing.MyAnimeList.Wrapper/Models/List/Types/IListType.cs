// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Types
{
	internal interface IListType<T> where T : class, IListEntry
	{
		abstract static ListEntryType ListEntryType { get; }
		abstract static string LatestUpdatesUrl(string username);
	}
}