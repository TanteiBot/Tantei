using System.Collections.Generic;
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
		public List<FeedItem> Items { get; set; } = null!;
	}
}