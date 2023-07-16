// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PaperMalKing.Common.Enums;
using static PaperMalKing.MyAnimeList.Wrapper.Abstractions.MangaFieldsToRequest;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;

public abstract class MangaListType : IListType
{
	public static ListEntryType ListEntryType => ListEntryType.Manga;

	public static string LatestUpdatesUrl<TRequestOptions>(string username, TRequestOptions options) where TRequestOptions : unmanaged, Enum
	{
		Debug.Assert(typeof(TRequestOptions) == typeof(MangaFieldsToRequest));
		var fields = Unsafe.As<TRequestOptions, MangaFieldsToRequest>(ref options);
		return
			$"/users/{username}/mangalist?fields=list_status{{status,score,num_volumes_read,num_chapters_read,is_rereading,num_times_reread,updated_at{(fields.HasFlag(Tags) ? ",tags" : "")}{(fields.HasFlag(Comments) ? ",comments" : "")}{(fields.HasFlag(Dates) ? ",start_date,finish_date" : "")}}},id,title,main_picture,media_type,status,num_volumes,num_chapters{(fields.HasFlag(Synopsis) ? ",synopsis" : "")}{(fields.HasFlag(Genres) ? ",genres{name}" : "")}{(fields.HasFlag(Authors) ? ",authors{first_name,last_name}" : "")}&limit=100&sort=list_updated_at&nsfw=true";
	}
}