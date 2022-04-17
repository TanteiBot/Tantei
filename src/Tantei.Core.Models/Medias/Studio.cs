// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Medias;

public sealed record Studio(string Name, Uri? Url) : INameUrlable;