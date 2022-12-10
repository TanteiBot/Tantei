// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss;

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