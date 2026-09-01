// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Converters;

public sealed class UnknownJsonStringEnumConverter<TEnum> : JsonConverter<TEnum>
	where TEnum : struct, Enum
{
	private static readonly JsonConverter<TEnum> InnerConverter =
		(JsonConverter<TEnum>)new JsonStringEnumConverter<TEnum>().CreateConverter(typeof(TEnum), new());

	public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		try
		{
			return InnerConverter.Read(ref reader, typeToConvert, options);
		}
		catch (JsonException) when (reader.TokenType == JsonTokenType.String)
		{
			return default;
		}
	}

	public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) => InnerConverter.Write(writer, value, options);
}
