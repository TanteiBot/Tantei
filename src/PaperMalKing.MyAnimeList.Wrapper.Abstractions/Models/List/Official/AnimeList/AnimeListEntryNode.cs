// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

public sealed class AnimeListEntryNode : BaseListEntryNode<AnimeMediaType, AnimeAiringStatus>
{
	private string? _url = null;
	public override uint TotalSubEntries => this.Episodes;

	[JsonPropertyName("num_episodes")]
	[JsonRequired]
	public uint Episodes { get; internal set; }

	[JsonPropertyName("studios")]
	public IReadOnlyList<Studio>? Studios { get; internal set; }

	public override string Url => this._url ??= $"{Constants.BASE_URL}/anime/{this.Id}";
}