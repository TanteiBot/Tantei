// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.Common.Converters;

public sealed class JsonToBoolConverter : JsonConverter<bool>
{
	/// <inheritdoc />
	public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
	{
		JsonTokenType.Number => reader.GetByte() == 1,
		JsonTokenType.String => reader.GetString() == "1",
		JsonTokenType.True => true,
		_ => false
	};

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
		writer.WriteBoolean("is_reprogressing", value);
}