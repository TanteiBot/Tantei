// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Net;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public class UserInfo
{
	private string? _imageUrl;

	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("nickname")]
	public required string Nickname { get; init; }

	public string Url => $"{Constants.BaseUrl}/{WebUtility.UrlEncode(this.Nickname)}";

	public string ImageUrl => this._imageUrl ??= Utils.GetImageUrl("users", this.Id, "png", "x80");
}