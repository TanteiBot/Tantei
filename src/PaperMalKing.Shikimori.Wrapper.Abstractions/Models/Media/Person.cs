// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public sealed class Person : IMultiLanguageName
{
	[JsonPropertyName("id")]
	[JsonRequired]
	public uint Id { get; internal set; }

	[JsonPropertyName("name")]
	[JsonRequired]
	public string Name { get; internal set; } = null!;

	[JsonPropertyName("russian")]
	[JsonRequired]
	public string RussianName { get; internal set; } = null!;

	public string Url => Utils.GetUrl("people", this.Id);
}