// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.Wrapper;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(AnimeMedia))]
[JsonSerializable(typeof(MangaMedia))]
[JsonSerializable(typeof(Favourites))]
[JsonSerializable(typeof(History))]
[JsonSerializable(typeof(History[]))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(UserInfo))]
[JsonSerializable(typeof(List<Role>))]
internal partial class JsonSGContext : JsonSerializerContext
{ }