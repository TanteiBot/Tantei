// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Numerics;

namespace PaperMalKing.Shikimori.Wrapper;

internal static class Utils
{
	public static string GetImageUrl<T>(string type, T id, string imageExt = "jpg", string size = "original") where T: unmanaged, IUnsignedNumber<T>, IBinaryInteger<T>
		=> $"{Constants.BASE_URL}/system/{type}/{size}/{id}.{imageExt}?{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

	public static string GetUrl(string type, uint id) => $"{Constants.BASE_URL}/{type}/{id}";
}