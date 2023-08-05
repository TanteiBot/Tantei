// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Globalization;
using System.Numerics;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions;

internal static class Utils
{
	public static string GetImageUrl<T>(string type, T id, string imageExt = "jpg", string size = "original") where T: unmanaged, IUnsignedNumber<T>, IBinaryInteger<T>
		=> string.Create(CultureInfo.InvariantCulture, $"{Constants.BASE_URL}/system/{type}/{size}/{id}.{imageExt}?{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");

	public static string GetUrl(string type, uint id) => $"{Constants.BASE_URL}/{type}/{id}";
}