// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

namespace PaperMalKing.MyAnimeList.Wrapper;

[JsonSerializable(typeof(ListQueryResult<AnimeListEntry, AnimeListEntryNode, AnimeListEntryStatus, AnimeMediaType, AnimeAiringStatus, AnimeListStatus>))]
[JsonSerializable(typeof(ListQueryResult<MangaListEntry, MangaListEntryNode, MangaListEntryStatus, MangaMediaType, MangaPublishingStatus, MangaListStatus>))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
internal sealed partial class JsonContext : JsonSerializerContext;