// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests;

internal static class MalEmbedTestLimits
{
	public const int UrlLimit = 2048;

	public const int DescriptionTextLimit = 500;

	public const string DescriptionField = "Description";

	public static string OverLongUrl() => $"https://cdn.myanimelist.net/{new string('u', UrlLimit)}.jpg";
}
