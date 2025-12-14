// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed record Paginatable<T>(T[] Data, bool HasNextPage)
	where T : class
{
	public static Paginatable<T> Empty { get; } = new Paginatable<T>([], HasNextPage: false);
}
