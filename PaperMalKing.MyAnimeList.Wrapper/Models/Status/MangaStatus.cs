// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper.Models.Status
{
	internal enum MangaStatus : byte
	{
		CurrentlyPublishing = 1,
		FinishedPublishing = 2,
		NotYetPublished = 3,
		OnHiatus = 4,
		Discontinued = 5
	}
}