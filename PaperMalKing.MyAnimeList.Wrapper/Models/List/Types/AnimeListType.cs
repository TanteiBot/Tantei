// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PaperMalKing.Common.Enums;
using static PaperMalKing.MyAnimeList.Wrapper.AnimeFieldsToRequest;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Types;

internal abstract class AnimeListType : IListType
{
	public static ListEntryType ListEntryType => ListEntryType.Anime;

	public static string LatestUpdatesUrl<TRequestOptions>(string username, TRequestOptions options) where TRequestOptions : unmanaged, Enum
	{
		Debug.Assert(typeof(TRequestOptions) == typeof(AnimeFieldsToRequest));
		var fields = Unsafe.As<TRequestOptions, AnimeFieldsToRequest>(ref options);
		return
			$"/users/{username}/animelist?fields=list_status{{status,score,num_episodes_watched,is_rewatching,num_times_rewatched,updated_at{(fields.Has(Tags) ? ",tags" : "")}{(fields.Has(Comments) ? ",comments" : "")}}},id,title,main_picture,media_type,status,num_episodes,studios{(fields.Has(Synopsis) ? ",synopsis" : "")}{(fields.Has(Genres) ? ",genres" : "")}&limit=1000&sort=list_updated_at&nsfw=true";
	}
}