// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed record UserAchievement([property: JsonPropertyName("neko_id")]string Id, [property: JsonPropertyName("level")]byte Level, [property: JsonPropertyName("progress")] byte Progress);