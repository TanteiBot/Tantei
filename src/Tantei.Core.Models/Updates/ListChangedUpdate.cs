// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Tantei.Core.Models.Medias;
using Tantei.Core.Models.Scoring;

namespace Tantei.Core.Models.Updates;

public sealed record ListChangedUpdate(Author Author, ProviderInfo Provider, Media Media, ListEntryStatus Status, BaseScoreSystem Score,
									   DateTimeOffset? TimeStamp = null) : BaseUserUpdate(Author, Provider, TimeStamp, Media.ImageUrl);