// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PaperMalKing.Common.Enums;
using static PaperMalKing.MyAnimeList.Wrapper.Abstractions.AnimeFieldsToRequest;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;

public abstract class AnimeListType : IListType
{
	public static ListEntryType ListEntryType => ListEntryType.Anime;

	[SuppressMessage("Major Code Smell", "S103:Lines should not be too long", Justification = "It should be a single line")]
	public static string LatestUpdatesUrl<TRequestOptions>(string username, TRequestOptions options)
		where TRequestOptions : unmanaged, Enum
	{
		Debug.Assert(typeof(TRequestOptions) == typeof(AnimeFieldsToRequest), $"Request options must be a {nameof(AnimeFieldsToRequest)}");
		var fields = Unsafe.As<TRequestOptions, AnimeFieldsToRequest>(ref options);
		var tags = fields.HasFlag(Tags) ? ",tags" : "";
		var comments = fields.HasFlag(Comments) ? ",comments" : "";
		var dates = fields.HasFlag(Dates) ? ",start_date,finish_date" : "";
		var synopsis = fields.HasFlag(Synopsis) ? ",synopsis" : "";
		var genres = fields.HasFlag(Genres) ? ",genres{name}" : "";
		return
			$"/users/{username}/animelist?fields=list_status{{status,score,num_episodes_watched,is_rewatching,num_times_rewatched,updated_at{tags}{comments}{dates}}},id,title,main_picture,media_type,status,num_episodes,studios{synopsis}{genres}&limit=100&sort=list_updated_at&nsfw=true";
	}
}