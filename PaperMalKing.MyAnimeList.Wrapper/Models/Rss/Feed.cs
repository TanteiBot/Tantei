// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss;

[XmlRoot(ElementName = "rss")]
public sealed class Feed
{
	[XmlElement(ElementName = "channel")]
	public FeedChannel Channel { get; set; } = null!;

	[XmlAttribute(AttributeName = "version")]
	public string Version { get; set; } = null!;

	[XmlIgnore]
	public IReadOnlyList<FeedItem> Items => this.Channel.Items;
}