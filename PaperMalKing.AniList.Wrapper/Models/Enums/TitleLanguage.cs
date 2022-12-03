#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.AniList.Wrapper.Models.Enums
{
	[SuppressMessage("Microsoft.Design", "CA1008")]
	[SuppressMessage("Naming", "CA1707")]
	public enum TitleLanguage : byte
    {
		NATIVE = 1,
        NATIVE_STYLISED = 2,
        ROMAJI = 3,
        ROMAJI_STYLISED = 4,
        ENGLISH = 5,
        ENGLISH_STYLISED = 6
    }
}