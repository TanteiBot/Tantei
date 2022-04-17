// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Tantei.Core.Models.Medias;

namespace Tantei.Core.Models.Updates;

public sealed record ReviewUpdate(Author Author, ProviderInfo Provider, string Content, Media Media, string? Name = null,
								  DateTimeOffset? TimeStamp = null, Uri? ImageUrl = null) : BaseUserUpdate(Author, Provider, TimeStamp, ImageUrl);