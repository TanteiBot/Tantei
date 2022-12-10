// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss;

[XmlRoot(ElementName = "channel")]
public sealed class FeedChannel
{
	[XmlElement(ElementName = "title")]
	public required string Title { get; set; }

	[XmlElement(ElementName = "link")]
	public required string Link { get; set; }

	[XmlElement(ElementName = "description")]
	public required string Description { get; set; }

	[XmlElement(ElementName = "item")]
	#pragma warning disable CA2227, CA1002
	public required List<FeedItem> Items { get; set; }
	#pragma warning restore CA2227, CA1002
}