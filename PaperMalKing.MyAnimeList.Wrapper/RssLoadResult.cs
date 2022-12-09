// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper;

internal enum RssLoadResult : short
{
	Forbidden = 403,
	NotFound = 404,
	Teapot = 418
}