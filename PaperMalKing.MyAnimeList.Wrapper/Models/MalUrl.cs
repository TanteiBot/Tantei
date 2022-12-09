// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using PaperMalKing.MyAnimeList.Wrapper.Parsers;

namespace PaperMalKing.MyAnimeList.Wrapper.Models
{
	internal sealed class MalUrl
	{
		private int? _id;
		internal string Url { get; }

		internal int Id => this._id ??= CommonParser.ExtractIdFromMalUrl(this.Url);

		public MalUrl(string url)
		{
			this.Url = url;
		}
	}
}