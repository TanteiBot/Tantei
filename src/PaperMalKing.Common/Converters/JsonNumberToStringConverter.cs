// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.Common.Converters;

public sealed class JsonNumberToStringConverter : JsonConverter<string>
{
	public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
	{
		JsonTokenType.Number => reader.GetDouble().ToString(CultureInfo.InvariantCulture),
		JsonTokenType.String => reader.GetString(),
		_ => ""
	};

	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
	{
		writer.WriteString("", value);
	}
}