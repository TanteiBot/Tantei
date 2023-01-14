// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.Media;

internal sealed class MangaMedia : BaseMedia
{
	[JsonPropertyName("publishers")]
	public IReadOnlyList<Publisher> Publishers { get; init; } = Array.Empty<Publisher>();
	protected override string Type => "mangas";
}