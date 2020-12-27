using System.Collections.Generic;
using System.Xml.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss
{
	[XmlRoot(ElementName = "rss")]
	public sealed class Feed
	{
		[XmlElement(ElementName = "channel")]
		public FeedChannel Channel { get; set; } = null!;

		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; } = null!;

		[XmlIgnore]
		public List<FeedItem> Items => this.Channel.Items;
	}
}