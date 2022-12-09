// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.Common.Converters;

public sealed class JsonToInterfaceConverter<TInterface, TImplementation> : JsonConverter<TInterface> where TImplementation : TInterface
{
	public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return JsonSerializer.Deserialize<TImplementation>(ref reader, options);
	}

	public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, (TImplementation)value!, typeof(TImplementation), options);
	}
}