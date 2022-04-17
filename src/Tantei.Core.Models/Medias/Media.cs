// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Medias;

public sealed record Media(string Name, Uri Url, IReadOnlyList<SubEntry> Entries, Format Format, Status Status, Uri? ImageUrl = null,
						   string? Notes = null, IReadOnlyList<Tag>? Tags = null, IReadOnlyList<string>? Genres = null, string? Description = null,
						   IReadOnlyList<Studio>? Studios = null, IReadOnlyCollection<Staff>? Staff = null) : INameUrlable, IHasImage;