// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace PaperMalKing.Common;

#pragma warning disable CA1724
public static class Helpers
#pragma warning restore
{
	private static readonly string EmptySha512Hash;

	[SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline")]
	[SuppressMessage("Minor Code Smell", "S3963:\"static\" fields should be initialized inline")]
	static Helpers()
	{
		using var incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
		EmptySha512Hash = FormatHash(0, incrementalHash.GetCurrentHash());
	}

	public static string ToDiscordMention(ulong id) => $"<@!{id}>";

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
			BinaryPrimitives.WriteUInt32LittleEndian(buffer,id.Id);

			buffer[^2] = id.Type;

			incrementalHash.AppendData(buffer);
		}

		Debug.Assert(incrementalHash.HashLengthInBytes == shaHashDestination.Length);
		incrementalHash.GetCurrentHash(shaHashDestination);
		return FormatHash(ids.Length, shaHashDestination);
	}

	private static string FormatHash(int length, Span<byte> shaHashDestination) => $"{length}:{Convert.ToHexString(shaHashDestination)}";
}