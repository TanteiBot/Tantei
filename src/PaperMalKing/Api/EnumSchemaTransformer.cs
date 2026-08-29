// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PaperMalKing.Api;

internal sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
	public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
	{
		var type = context.JsonTypeInfo.Type;
		if (!type.IsEnum)
		{
			return Task.CompletedTask;
		}

		var names = new JsonArray();
		schema.Enum = [];
		foreach (var value in Enum.GetValues(type))
		{
			schema.Enum.Add(JsonValue.Create(Convert.ToInt32(value, CultureInfo.InvariantCulture)));
			names.Add(JsonValue.Create(Enum.GetName(type, value)));
		}

		schema.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
		schema.Extensions["x-enum-varnames"] = new JsonNodeExtension(names);

		return Task.CompletedTask;
	}
}
