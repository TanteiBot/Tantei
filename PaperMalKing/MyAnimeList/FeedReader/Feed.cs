using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PaperMalKing.MyAnimeList.FeedReader
{
	/// <summary>
	/// MyAnimeList user's RSS feed item
	/// </summary>
	[XmlRoot(ElementName = "item")]
	public sealed class FeedItem
	{
		/// <summary>
		/// Contains title of entity and its type divided by " - "
		/// </summary>
		[XmlElement(ElementName = "title")]
		public string Title { get; set; }

		/// <summary>
		/// Link to entity on MyAnimeList
		/// </summary>
		[XmlElement(ElementName = "link")]
		public string Link { get; set; }

		/// <summary>
		/// <see cref="Link"/>
		/// </summary>
		[XmlElement(ElementName = "guid")]
		public string Guid { get; set; }

		/// <summary>
		/// Contains info about update such as status of entity and amount of watched/read episodes/chapters
		/// <example>Completed - 12 of 12 episodes</example>
		/// </summary>
		[XmlElement(ElementName = "description")]
		public string Description { get; set; }

		/// <summary>
		/// String with date and time when user 'applied' update
		/// </summary>
		[XmlElement(ElementName = "pubDate")]
		public string PubDateString { get; set; }

		[XmlIgnore]
		private bool? _isPlanToCheck;

		[XmlIgnore]
		public bool IsPlanToCheck
		{
			get
			{
				if (!this._isPlanToCheck.HasValue)
				{
					this._isPlanToCheck =
						this.Description.Contains("plan to", StringComparison.InvariantCultureIgnoreCase);
				}

				return this._isPlanToCheck.Value;
			}
		}

		[XmlIgnore]
		public DateTime PublishingDateTime { get; set; }
	}

	[XmlRoot(ElementName = "channel")]
	public sealed class FeedChannel
	{
		[XmlElement(ElementName = "title")]
		public string Title { get; set; }

		[XmlElement(ElementName = "link")]
		public string Link { get; set; }

		[XmlElement(ElementName = "description")]
		public string Description { get; set; }

		[XmlElement(ElementName = "item")]
		public List<FeedItem> Items { get; set; }
	}

	[XmlRoot(ElementName = "rss")]
	public sealed class Feed
	{
		[XmlElement(ElementName = "channel")]
		public FeedChannel Channel { get; set; }

		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; }

		[XmlIgnore]
		public List<FeedItem> Items => this.Channel.Items;

		public void TryFillDateTime()
		{
			foreach (var feedItem in this.Items)
			{
				DateTime pubDate = DateTime.MinValue;
				if (DateTime.TryParse(feedItem.PubDateString, out var result))
					pubDate = result;
				feedItem.PublishingDateTime = pubDate.ToUniversalTime();
			}
		}


		public EntityType GetEntitiesType()
		{
			return this.Channel.Description.Contains("anime", StringComparison.InvariantCultureIgnoreCase)
				? EntityType.Anime
				: EntityType.Manga;
		}
	}
}