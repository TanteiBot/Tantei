// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.Common.Json;

public sealed class BoolWrapperConverter<T> : JsonConverter<T>
	where T : IBoolWrapper<T>
{
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		Debug.Assert(reader.TokenType == JsonTokenType.StartObject, "Must be an object");
		_ = reader.Read();
		Debug.Assert(reader.TokenType == JsonTokenType.PropertyName, "Must contain a property name");
		_ = reader.Read();
		var returnValue = reader.TokenType switch
		{
			JsonTokenType.True => T.TrueValue,
			JsonTokenType.False => T.FalseValue,
			_ => throw new JsonException(),
		};
		_ = reader.Read();
		Debug.Assert(reader.TokenType == JsonTokenType.EndObject, "Must be an object");
		return returnValue;
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
	}
}