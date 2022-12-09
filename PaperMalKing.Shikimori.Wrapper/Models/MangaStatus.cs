// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.ComponentModel;

namespace PaperMalKing.Shikimori.Wrapper.Models
{
	public enum MangaStatus: byte
	{
		[Description("Читаю")]
		Reading = 0,
		[Description("Прочитано")]
		Completed = 1,
		[Description("Отложено")]
		OnHold = 2,
		[Description("Брошено")]
		Dropped = 3,
		[Description("Запланировано")]
		Planned = 4,
		[Description("Перечитываю")]
		Rereading = 5
	}
}