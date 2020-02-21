using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PaperMalKing.MyAnimeList.FeedReader
{
    [XmlRoot(ElementName = "item")]
    public sealed class FeedItem
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "link")]
        public string Link { get; set; }

        [XmlElement(ElementName = "guid")]
        public string Guid { get; set; }

        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        [XmlElement(ElementName = "pubDate")]
        public string PubDateString { get; set; }

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