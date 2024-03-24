// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PaperMalKing.Common.Enums;
using static PaperMalKing.MyAnimeList.Wrapper.Abstractions.AnimeFieldsToRequest;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;

public abstract class AnimeListType : IListType
{
	public static ListEntryType ListEntryType => ListEntryType.Anime;

	public static string LatestUpdatesUrl<TRequestOptions>(string username, TRequestOptions options)
		where TRequestOptions : unmanaged, Enum
	{
		Debug.Assert(typeof(TRequestOptions) == typeof(AnimeFieldsToRequest), $"Request options must be a {nameof(AnimeFieldsToRequest)}");
		var fields = Unsafe.As<TRequestOptions, AnimeFieldsToRequest>(ref options);
		return
			$"/users/{username}/animelist?fields=list_status{{status,score,num_episodes_watched,is_rewatching,num_times_rewatched,updated_at{(fields.HasFlag(Tags) ? ",tags" : "")}{(fields.HasFlag(Comments) ? ",comments" : "")}{(fields.HasFlag(Dates) ? ",start_date,finish_date" : "")}}},id,title,main_picture,media_type,status,num_episodes,studios{(fields.HasFlag(Synopsis) ? ",synopsis" : "")}{(fields.HasFlag(Genres) ? ",genres{name}" : "")}&limit=100&sort=list_updated_at&nsfw=true";
	}
}