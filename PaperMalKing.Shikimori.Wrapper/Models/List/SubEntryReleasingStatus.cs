// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.ComponentModel;

namespace PaperMalKing.Shikimori.Wrapper.Models.List;

public enum SubEntryReleasingStatus : byte
{
	[Description("Выпущено")]
	Released = 0,

	[Description("Онгоинг")]
	Ongoing = 2,

	[Description("Анонс")]
	Anons = 3
}