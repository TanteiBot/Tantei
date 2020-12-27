using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Models;

namespace PaperMalKing.MyAnimeList.Wrapper
{
	public sealed class JsonReprogressingConverter : JsonConverter<Reprogressing>
	{
		/// <inheritdoc />
		public override Reprogressing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
		{
			JsonTokenType.Number => new(reader.GetByte()),
			JsonTokenType.String => new(reader.GetString()!),
			JsonTokenType.False  => new(false),
			JsonTokenType.True   => new(true),
			_                    => new()
		};

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, Reprogressing value, JsonSerializerOptions options) =>
			writer.WriteBoolean("is_reprogressing", value.IsReprogressing);

		public JsonReprogressingConverter() {}
	}
}