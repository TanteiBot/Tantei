// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Runtime.InteropServices;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

[StructLayout(LayoutKind.Auto)]
public readonly record struct TenraiAttemptFacts(int RetryCount, TimeSpan? RetryAfter);
