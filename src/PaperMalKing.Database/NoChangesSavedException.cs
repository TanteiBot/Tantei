// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;

namespace PaperMalKing.Database;

public sealed class NoChangesSavedException() : Exception("No changes were saved to database");