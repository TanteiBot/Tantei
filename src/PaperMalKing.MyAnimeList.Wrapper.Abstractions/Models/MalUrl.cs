// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;

public sealed class MalUrl
{
	private uint? _id;
	public  string Url { get; }

	public uint Id => this._id ??= Helper.ExtractIdFromMalUrl(this.Url);

	public MalUrl(string url)
	{
		this.Url = url;
	}
}