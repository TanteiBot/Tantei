// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.ComponentModel;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

public enum MangaPublishingStatus: byte
{
	unknown = 0,
	[Description("Finished")]
	finished = 1,
	[Description("Currently publishing")]
	currently_publishing = 2,
	[Description("Not yet published")]
	not_yet_published = 3,
	[Description("Discontinued")]
	discontinued = 4,
	[Description("Hiatus")]
	on_hiatus = 5
}