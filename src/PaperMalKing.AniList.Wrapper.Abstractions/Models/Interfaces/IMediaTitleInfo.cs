// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Interfaces;

internal interface IMediaTitleInfo
{
	MediaTitle Title { get; }

	MediaFormat? Format { get; }

	string? CountryOfOrigin { get; }

	MediaStatus Status { get; }
}
