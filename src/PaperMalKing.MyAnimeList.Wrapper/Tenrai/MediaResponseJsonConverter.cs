// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class MediaResponseJsonConverter : JsonConverter<MediaResponse>
{
	public override MediaResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		if (document.RootElement.ValueKind is not JsonValueKind.Object ||
			!document.RootElement.TryGetProperty("data", out var data) ||
			data.ValueKind is not JsonValueKind.Object)
		{
			return new();
		}

		return new()
		{
			Data = new()
			{
				Themes = ReadCatalog(data, "themes"),
				Demographics = ReadCatalog(data, "demographics"),
			},
		};
	}

	public override void Write(Utf8JsonWriter writer, MediaResponse value, JsonSerializerOptions options) =>
		throw new NotSupportedException();

	private static List<CatalogReference> ReadCatalog(JsonElement data, string propertyName)
	{
		if (!data.TryGetProperty(propertyName, out var property) || property.ValueKind is not JsonValueKind.Array)
		{
			return [];
		}

		var entries = new List<CatalogReference>();
		foreach (var entry in property.EnumerateArray())
		{
			if (entry.ValueKind is JsonValueKind.Object &&
				entry.TryGetProperty("name", out var name) &&
				name.ValueKind is JsonValueKind.String &&
				!string.IsNullOrWhiteSpace(name.GetString()))
			{
				entries.Add(new() { Name = name.GetString(), });
			}
		}

		return entries;
	}
}
