#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
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

using PaperMalKing.MyAnimeList.Wrapper.Parsers;

namespace PaperMalKing.MyAnimeList.Wrapper.Models
{
	internal sealed class MalUrl
	{
		private int? _id;
		internal string Url { get; }

		internal int Id => this._id ??= CommonParser.ExtractIdFromMalUrl(this.Url);

		public MalUrl(string url)
		{
			this.Url = url;
		}
	}
}