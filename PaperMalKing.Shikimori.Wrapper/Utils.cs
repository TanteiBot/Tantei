// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;

namespace PaperMalKing.Shikimori.Wrapper;

internal static class Utils
{
	public static string GetImageUrl(string type, ulong id, string imageExt = "jpg", string size = "original") =>
		$"{Constants.BASE_URL}/system/{type}/{size}/{id}.{imageExt}?{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

	public static string GetUrl(string type, ulong id) => $"{Constants.BASE_URL}/{type}/{id}";
}