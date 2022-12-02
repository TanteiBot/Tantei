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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss
{
	[XmlRoot(ElementName = "channel")]
	public sealed class FeedChannel
	{
		[XmlElement(ElementName = "title")]
		public string Title { get; set; } = null!;

		[XmlElement(ElementName = "link")]
		public string Link { get; set; } = null!;

		[XmlElement(ElementName = "description")]
		public string Description { get; set; } = null!;

		[XmlElement(ElementName = "item")]
		#pragma warning disable CA2227, CA1002
		public List<FeedItem> Items { get; set; } = null!;
		#pragma warning restore CA2227, CA1002
	}
}