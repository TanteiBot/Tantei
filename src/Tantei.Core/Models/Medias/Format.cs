// Tantei.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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