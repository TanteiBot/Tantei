// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

[SuppressMessage("Performance", "CA1852:Seal internal types", Justification = "The generated partial declaration contains virtual members")]
internal partial class TenraiClient
{
	static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings) =>
		settings.Converters.Add(new MediaResponseJsonConverter());
}
