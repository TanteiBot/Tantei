// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Text.Json;

namespace PaperMalKing.AniList.Wrapper
{
    public class JsonUpperPolicyCase : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToUpperInvariant();
    }
}