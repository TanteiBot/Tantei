// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Updates;

public sealed record Author(string Name, Uri? Url, Uri? AvatarUrl);