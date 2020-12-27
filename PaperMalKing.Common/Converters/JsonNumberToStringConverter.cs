﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.Common.Converters
{
	public sealed class JsonNumberToStringConverter : JsonConverter<string>
	{
		/// <inheritdoc />
		public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
		{
			JsonTokenType.Number => reader.GetInt32().ToString(),
			JsonTokenType.String => reader.GetString(),
			_                    => ""
		};

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
		{
			writer.WriteString("", value);
		}
	}
}