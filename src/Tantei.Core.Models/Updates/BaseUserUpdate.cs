// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N


namespace Tantei.Core.Models.Updates;

public abstract record BaseUserUpdate(Author Author, ProviderInfo Provider, DateTimeOffset? TimeStamp = null, Uri? ImageUrl = null) : IHasImage;