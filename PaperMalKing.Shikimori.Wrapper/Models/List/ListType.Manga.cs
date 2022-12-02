// Tantei.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using PaperMalKing.Common.Enums;

namespace PaperMalKing.Shikimori.Wrapper.Models.List;

#pragma warning disable MA0048
internal readonly struct MangaListType : IListType
{
	public string ListType => "manga_rates";

	public ListEntryType ListEntryType => ListEntryType.Manga;
}