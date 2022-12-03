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

using System;
using System.Xml.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss
{
	/// <summary>
	///     MyAnimeList user's RSS feed item
	/// </summary>
	[XmlRoot(ElementName = "item")]
	public sealed class FeedItem
	{
		[XmlIgnore]
		private bool? _isPlanToCheck;

		[XmlIgnore]
		private DateTimeOffset? _publishingDateTimeOffset;

		/// <summary>
		///     Contains title of entity and its type divided by " - "
		/// </summary>
		[XmlElement(ElementName = "title")]
		public string Title { get; set; } = null!;

		/// <summary>
		///     Link to entity on MyAnimeList
		/// </summary>
		[XmlElement(ElementName = "link")]
		public string Link { get; set; } = null!;

		/// <summary>
		///     <see cref="Link" />
		/// </summary>
		[XmlElement(ElementName = "guid")]
		[Obsolete("Use link property")]
		public string ItemGuid { get; set; } = null!;

		/// <summary>
		///     Contains info about update such as status of entity and amount of watched/read episodes/chapters
		///     <example>Completed - 12 of 12 episodes</example>
		/// </summary>
		[XmlElement(ElementName = "description")]
		public string Description { get; set; } = null!;

		/// <summary>
		///     String with date and time when user 'applied' update
		/// </summary>
		[XmlElement(ElementName = "pubDate")]
		public string PubDateString { get; set; } = null!;

		[XmlIgnore]
		public bool IsPlanToCheck =>
			this._isPlanToCheck ??= this.Description.Contains("plan to", StringComparison.OrdinalIgnoreCase);

		[XmlIgnore]
		public DateTimeOffset PublishingDateTimeOffset =>
			this._publishingDateTimeOffset ??= DateTimeOffset.Parse(this.PubDateString).ToUniversalTime();
	}
}