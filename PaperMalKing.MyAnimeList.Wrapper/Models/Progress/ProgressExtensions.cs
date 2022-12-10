// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

internal static class ProgressExtensions
{
	internal static GenericProgress ToGeneric(this MangaProgress @this) => (GenericProgress)@this;

	internal static GenericProgress ToGeneric(this AnimeProgress @this) => (GenericProgress)@this;

	internal static MangaProgress ToMangaProgress(this GenericProgress @this) => (MangaProgress)@this;

	internal static AnimeProgress ToAnimeProgress(this GenericProgress @this) => (AnimeProgress)@this;
}