// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PaperMalKing.Common.Enums;
using static PaperMalKing.MyAnimeList.Wrapper.Abstractions.MangaFieldsToRequest;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;

public abstract class MangaListType : IListType
{
	public static ListEntryType ListEntryType => ListEntryType.Manga;

	[SuppressMessage("Major Code Smell", "S103:Lines should not be too long", Justification = "It shouls be a single line")]
	public static string LatestUpdatesUrl<TRequestOptions>(string username, TRequestOptions options)
		where TRequestOptions : unmanaged, Enum
	{
		Debug.Assert(typeof(TRequestOptions) == typeof(MangaFieldsToRequest), $"Request options must be a {nameof(MangaFieldsToRequest)}");
		var fields = Unsafe.As<TRequestOptions, MangaFieldsToRequest>(ref options);
		var tags = fields.HasFlag(Tags) ? ",tags" : "";
		var comments = fields.HasFlag(Comments) ? ",comments" : "";
		var dates = fields.HasFlag(Dates) ? ",start_date,finish_date" : "";
		var synopsis = fields.HasFlag(Synopsis) ? ",synopsis" : "";
		var genres = fields.HasFlag(Genres) ? ",genres{name}" : "";
		var authors = fields.HasFlag(Authors) ? ",authors{first_name,last_name}" : "";
		return
			$"/users/{username}/mangalist?fields=list_status{{status,score,num_volumes_read,num_chapters_read,is_rereading,num_times_reread,updated_at{tags}{comments}{dates}}},id,title,main_picture,media_type,status,num_volumes,num_chapters{synopsis}{genres}{authors}&limit=100&sort=list_updated_at&nsfw=true";
	}
}