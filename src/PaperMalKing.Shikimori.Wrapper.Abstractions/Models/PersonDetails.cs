// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed class PersonDetails
{
	[JsonPropertyName("works")]
	public IReadOnlyList<PersonWork>? Works { get; init; }

	[JsonPropertyName("roles")]
	public IReadOnlyList<PersonRoleGroup>? Roles { get; init; }
}

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "Parts of one payload")]
public sealed class PersonWork
{
	[JsonPropertyName("anime")]
	public RelatedMedia? Anime { get; init; }

	[JsonPropertyName("manga")]
	public RelatedMedia? Manga { get; init; }

	[JsonIgnore]
	public RelatedMedia? Media => this.Anime ?? this.Manga;
}

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "Parts of one payload")]
public sealed class PersonRoleGroup
{
	[JsonPropertyName("animes")]
	public IReadOnlyList<RelatedMedia>? Animes { get; init; }

	[JsonPropertyName("mangas")]
	public IReadOnlyList<RelatedMedia>? Mangas { get; init; }

	[JsonIgnore]
	public RelatedMedia? Media => (this.Animes ?? []).Concat(this.Mangas ?? []).FirstOrDefault();
}
