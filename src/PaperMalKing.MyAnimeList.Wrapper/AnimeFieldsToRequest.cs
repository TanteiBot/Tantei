// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;

namespace PaperMalKing.MyAnimeList.Wrapper;

[Flags]
internal enum AnimeFieldsToRequest : byte
{
	Synopsis = 1,
	Genres = 1 << 1,
	Tags = 1 << 2,
	Comments = 1 << 3,
	Dates = 1 << 4,
	Studio = 1 << 5,
}