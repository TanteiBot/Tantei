// Tantei.
// Copyright (C) 2021-2022 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;

namespace PaperMalKing.AniList.Wrapper.Models
{
	public sealed class MediaListGroup
	{
		public IReadOnlyList<MediaListEntry> Entries { get; init; } = Array.Empty<MediaListEntry>();
	}
}