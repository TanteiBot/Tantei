// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json;
using System.Text.Json.Serialization;
using PaperMalKing.Api.Contracts.Responses;

namespace PaperMalKing.Api;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(LicenseResponse[]))]
internal sealed partial class CreditsJsonContext : JsonSerializerContext;
