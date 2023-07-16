// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.ComponentModel;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public enum AnimeStatus : byte
{
	[Description("Смотрю")]
	Watching = 0,
	[Description("Просмотрено")]
	Completed = 1,
	[Description("Отложено")]
	OnHold = 2,
	[Description("Брошено")]
	Dropped = 3,
	[Description("Запланировано")]
	Planned = 4,
	[Description("Пересматриваю")]
	Rewatching = 5
}