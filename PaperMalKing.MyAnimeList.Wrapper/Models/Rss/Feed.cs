// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss;

[XmlRoot(ElementName = "rss")]
public sealed class Feed
{
	[XmlElement(ElementName = "channel")]
	public required FeedChannel Channel { get; set; }

	[XmlAttribute(AttributeName = "version")]
	public required string Version { get; set; }

	[XmlIgnore]
	public IReadOnlyList<FeedItem> Items => this.Channel.Items;
}