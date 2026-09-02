// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class CharactersResponseJsonConverter : JsonConverter<CharactersResponse>
{
	public override CharactersResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		if (document.RootElement.ValueKind is not JsonValueKind.Object ||
			!document.RootElement.TryGetProperty("data", out var data) ||
			data.ValueKind is not JsonValueKind.Array)
		{
			return new() { Data = null!, };
		}

		var characters = new List<Character>();
		foreach (var characterElement in data.EnumerateArray())
		{
			if (characterElement.ValueKind is not JsonValueKind.Object ||
				!characterElement.TryGetProperty("voice_actors", out var actorElements) ||
				actorElements.ValueKind is not JsonValueKind.Array)
			{
				continue;
			}

			var actors = new List<VoiceActor>();
			foreach (var actorElement in actorElements.EnumerateArray())
			{
				if (actorElement.ValueKind is not JsonValueKind.Object)
				{
					continue;
				}

				actors.Add(new()
				{
					Language = ReadString(actorElement, "language"),
					Person = ReadPerson(actorElement),
				});
			}

			characters.Add(new() { Voice_actors = actors, });
		}

		return new() { Data = characters, };
	}

	public override void Write(Utf8JsonWriter writer, CharactersResponse value, JsonSerializerOptions options) =>
		throw new NotSupportedException();

	private static PersonReference? ReadPerson(JsonElement actor)
	{
		if (!actor.TryGetProperty("person", out var person) || person.ValueKind is not JsonValueKind.Object)
		{
			return null;
		}

		return new()
		{
			Name = ReadString(person, "name"),
			Url = ReadString(person, "url"),
		};
	}

	private static string? ReadString(JsonElement parent, string propertyName) =>
		parent.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.String
			? property.GetString()
			: null;
}
