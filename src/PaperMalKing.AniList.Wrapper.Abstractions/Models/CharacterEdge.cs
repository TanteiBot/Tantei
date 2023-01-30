// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class CharacterEdge
{
	[JsonPropertyName("voiceActors")]
	public IReadOnlyList<Staff> VoiceActors { get; init; } = Array.Empty<Staff>();
}