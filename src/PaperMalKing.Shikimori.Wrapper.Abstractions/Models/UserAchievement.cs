// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed record UserAchievement([property: JsonPropertyName("neko_id"), JsonConverter(typeof(StringPoolingJsonConverter))] string Id, [property: JsonPropertyName("level")] byte Level);