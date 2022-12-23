// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PaperMalKing.Common;

public static class Helpers
{
	public static string ToDiscordMention(ulong id) => $"<@!{id}>";

	public static string FavoritesHash(IReadOnlyList<FavoriteIdType> ids)
	{
		scoped Span<byte> shaHashDestination = stackalloc byte[SHA256.HashSizeInBytes];
		const int maxIdLength = 15; // "4294967295,255)".Length

		const int maxOnStackSize = 512;
		var count = (ids.Count * maxIdLength) + 1;
		scoped Span<byte> asciiDestination = count > maxOnStackSize ? new byte[maxOnStackSize] : stackalloc byte[maxOnStackSize];
		var idsString = string.Join("", ids.OrderBy(x=>x.Id).Select(x => $"{x.Id},{x.Type})"));
		Encoding.ASCII.GetBytes(idsString, asciiDestination);
		SHA256.HashData(asciiDestination, shaHashDestination);
		return Convert.ToHexString(shaHashDestination);
	}
}