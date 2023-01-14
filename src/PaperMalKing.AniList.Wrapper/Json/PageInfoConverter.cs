// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models;

namespace PaperMalKing.AniList.Wrapper.Json;

internal sealed class PageInfoConverter : JsonConverter<PageInfo>
{
	private static readonly PageInfo hasNextPage = new PageInfo()
	{
		HasNextPage = true
	};

	private static readonly PageInfo doesntHaveNextPage = new PageInfo()
	{
		HasNextPage = false
	};

	public override PageInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		Debug.Assert(reader.TokenType == JsonTokenType.StartObject);
		reader.Read();
		Debug.Assert(reader.TokenType == JsonTokenType.PropertyName);
		reader.Read();
		var returnValue =  reader.TokenType switch
		{
			JsonTokenType.True  => hasNextPage,
			JsonTokenType.False => doesntHaveNextPage,
			_                   => throw new JsonException()
		};
		reader.Read();
		Debug.Assert(reader.TokenType == JsonTokenType.EndObject);
		return returnValue;
	}


	public override void Write(Utf8JsonWriter writer, PageInfo value, JsonSerializerOptions options)
	{ }
}