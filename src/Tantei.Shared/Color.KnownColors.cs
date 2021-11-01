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

namespace Tantei.Shared;

public partial struct Color
{
	public static class AniList
	{
		public static readonly Color Green = new(0x7bd555);

		public static readonly Color Orange = new(0xf79a63);

		public static readonly Color Red = new(0xe85d75);

		public static readonly Color Peach = new(0xfa7a7a);

		public static readonly Color Blue = new(0x3db4f2);
	}

	public static class Shikimori
	{
		public static readonly Color Green = new(0x419541);

		public static readonly Color Red = new(0xFC575E);

		public static readonly Color Grey = new(0x7b8084);

		public static readonly Color Blue = new(0x176093);
	}

	public static class MyAnimeList
	{
		public static readonly Color Green = new(0x2db039);

		public static readonly Color Blue = new(0x26448f);

		public static readonly Color Yellow = new(0xf9d457);

		public static readonly Color Red = new(0xa12f31);

		public static readonly Color Grey = new(0xc3c3c3);
	}

	public static readonly Color Black = new(0x23272A);
}