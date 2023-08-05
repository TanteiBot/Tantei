// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class MediaListGroup
{
	public IReadOnlyList<MediaListEntry> Entries { get; init; } = Array.Empty<MediaListEntry>();
}