// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Xml.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss;

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
	public required string Title { get; set; }

	/// <summary>
	///     Link to entity on MyAnimeList
	/// </summary>
	[XmlElement(ElementName = "link")]
	public required string Link { get; set; }

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
	public required string Description { get; set; }

	/// <summary>
	///     String with date and time when user 'applied' update
	/// </summary>
	[XmlElement(ElementName = "pubDate")]
	public required string PubDateString { get; set; }

	[XmlIgnore]
	public bool IsPlanToCheck =>
		this._isPlanToCheck ??= this.Description.Contains("plan to", StringComparison.OrdinalIgnoreCase);

	[XmlIgnore]
	public DateTimeOffset PublishingDateTimeOffset =>
		this._publishingDateTimeOffset ??= DateTimeOffset.Parse(this.PubDateString).ToUniversalTime();
}