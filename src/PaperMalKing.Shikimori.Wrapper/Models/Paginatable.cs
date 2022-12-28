// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.Shikimori.Wrapper.Models;

internal sealed class Paginatable<T>
{
	public T Data { get; }

	public bool HasNextPage { get; }

	public Paginatable(T data, bool hasNextPage)
	{
		this.Data = data;
		this.HasNextPage = hasNextPage;
	}

	public void Deconstruct(out T data, out bool hasNextPage)
	{
		data = this.Data;
		hasNextPage = this.HasNextPage;
	}
}