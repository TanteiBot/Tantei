// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.ComponentModel;

namespace Tantei.Core.Models.Medias;

public enum Format : byte
{
	Unknown = 0,
	TV = 1,

	[Description("TV Short")]
	TVShort = 2,
	Movie = 3,
	Special = 4,
	OVA = 5,
	ONA = 6,
	Music = 7,
	Manga = 8,
	Novel = 9,

	[Description("Light Novel")]
	LightNovel = 10,

	[Description("One-Shot")]
	OneShot = 11,
	Doujinshi = 12,
	Manhwa = 13,
	Manhua = 14
}