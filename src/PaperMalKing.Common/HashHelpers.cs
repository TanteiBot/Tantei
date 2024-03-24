// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;

namespace PaperMalKing.Common;

public static class HashHelpers
{
	private static readonly string EmptySha512Hash = CreateEmptyHash();

	public static string FavoritesHash(Span<FavoriteIdType> ids)
	{
		if (ids.IsEmpty)
		{
			return EmptySha512Hash;
		}

		Span<byte> shaHashDestination = stackalloc byte[SHA512.HashSizeInBytes];
		Span<byte> buffer = stackalloc byte[sizeof(uint) + 1 + sizeof(byte) + 1];
		ids.Sort();
		buffer[^3] = (byte)',';
		buffer[^1] = (byte)')';

		using var incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
		foreach (var id in ids)
		{
			BinaryPrimitives.WriteUInt32LittleEndian(buffer, id.Id);

			buffer[^2] = id.Type;

			incrementalHash.AppendData(buffer);
		}

		Debug.Assert(incrementalHash.HashLengthInBytes == shaHashDestination.Length, "Length of hash did not match destination");
		_ = incrementalHash.GetCurrentHash(shaHashDestination);
		return FormatHash(ids.Length, shaHashDestination);
	}

	private static string FormatHash(int length, Span<byte> shaHashDestination)
	{
		return string.Create(CultureInfo.InvariantCulture, $"{length}:{Convert.ToHexString(shaHashDestination)}");
	}

	private static string CreateEmptyHash()
	{
		using var incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
		return FormatHash(0, incrementalHash.GetCurrentHash());
	}
}