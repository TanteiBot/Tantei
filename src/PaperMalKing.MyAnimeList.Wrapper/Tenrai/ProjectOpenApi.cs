// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

#:property OutputType=Exe
#:property NoWarn=$(NoWarn);MA0048

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProjectOpenApi;

internal static class Program
{
	private const string RetryAfterHeader = "Retry-After";
	private const string SchemaProperty = "schema";

	private static readonly OperationContract[] Operations =
	[
		new("/anime/{id}", "getAnimeById", "MediaResponse"),
		new("/manga/{id}", "getMangaById", "MediaResponse"),
		new("/anime/{id}/characters", "getAnimeByIdCharacters", "CharactersResponse"),
	];

	private static readonly string[] ErrorStatuses = ["400", "401", "403", "404", "405", "429", "500", "502", "503", "504"];

	public static void Main(string[] args)
	{
		if (args.Length != 2)
		{
			throw new ArgumentException("The source and output OpenAPI paths are required.", nameof(args));
		}

		var sourceText = File.ReadAllText(args[0]);
		using var sourceDocument = JsonDocument.Parse(sourceText);
		ValidateNoDuplicateProperties(sourceDocument.RootElement, "$");

		var source = JsonNode.Parse(sourceText)?.AsObject() ?? throw new InvalidDataException("The source OpenAPI document is empty.");
		ValidateDocument(source);
		var output = CreateProjection(source);
		File.WriteAllText(args[1], output.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
	}

	private static void ValidateDocument(JsonObject source)
	{
		var openApiVersion = GetRequiredString(source, "openapi", "the OpenAPI version");
		if (!openApiVersion.StartsWith("3.", StringComparison.Ordinal))
		{
			throw new InvalidDataException($"Unsupported OpenAPI version '{openApiVersion}'.");
		}

		_ = GetRequiredString(source["info"]?.AsObject(), "version", "the API version");
		var servers = source["servers"]?.AsArray() ?? throw new InvalidDataException("Missing servers.");
		if (servers.Count != 1 || !string.Equals(servers[0]?["url"]?.GetValue<string>(), "https://api.tenrai.org/v1", StringComparison.Ordinal))
		{
			throw new InvalidDataException("Unexpected Tenrai v1 server contract.");
		}

		var paths = source["paths"]?.AsObject() ?? throw new InvalidDataException("Missing paths.");
		foreach (var contract in Operations)
		{
			ValidateOperation(paths, contract);
		}
	}

	private static void ValidateOperation(JsonObject paths, OperationContract contract)
	{
		var operation = paths[contract.Path]?["get"]?.AsObject() ?? throw new InvalidDataException($"Missing GET operation {contract.Path}.");
		var actualOperationId = operation["operationId"]?.GetValue<string>();
		if (!string.Equals(actualOperationId, contract.OperationId, StringComparison.Ordinal))
		{
			throw new InvalidDataException($"Expected operationId '{contract.OperationId}' for GET {contract.Path}, but found '{actualOperationId ?? "<missing>"}'.");
		}

		var occurrences = paths
			.Where(static path => path.Value is JsonObject)
			.SelectMany(static path => path.Value!.AsObject())
			.Count(candidate => candidate.Value is JsonObject candidateObject
				&& string.Equals(candidateObject["operationId"]?.GetValue<string>(), contract.OperationId, StringComparison.Ordinal));
		if (occurrences != 1)
		{
			throw new InvalidDataException($"Expected operationId '{contract.OperationId}' exactly once, but found {occurrences.ToString(CultureInfo.InvariantCulture)} occurrences.");
		}

		var responses = operation["responses"]?.AsObject() ?? throw new InvalidDataException($"Missing responses for GET {contract.Path}.");
		var expectedStatuses = ErrorStatuses.Append("200").ToHashSet(StringComparer.Ordinal);
		var actualStatuses = responses.Select(static response => response.Key).ToHashSet(StringComparer.Ordinal);
		if (!actualStatuses.SetEquals(expectedStatuses))
		{
			var expected = string.Join(", ", expectedStatuses.Order(StringComparer.Ordinal));
			var actual = string.Join(", ", actualStatuses.Order(StringComparer.Ordinal));
			throw new InvalidDataException(
				$"Unexpected response statuses for GET {contract.Path}. Expected {expected}; found {actual}.");
		}

		foreach (var status in expectedStatuses)
		{
			var response = responses[status]?.AsObject() ?? throw new InvalidDataException($"Missing response {status} for GET {contract.Path}.");
			_ = GetRequiredString(response, "description", $"the {status} response description for GET {contract.Path}");
			_ = response["content"]?["application/json"]?[SchemaProperty]?.AsObject()
				?? throw new InvalidDataException($"Missing application/json schema for GET {contract.Path} status {status}.");
			ValidateResponseHeaders(contract.Path, status, response);
		}
	}

	private static void ValidateResponseHeaders(string path, string status, JsonObject response)
	{
		if (!string.Equals(status, "429", StringComparison.Ordinal) && !string.Equals(status, "503", StringComparison.Ordinal))
		{
			return;
		}

		var headers = response["headers"]?.AsObject() ?? throw new InvalidDataException($"Missing {RetryAfterHeader} header for GET {path} status {status}.");
		var occurrences = headers.Count(static header => string.Equals(header.Key, RetryAfterHeader, StringComparison.OrdinalIgnoreCase));
		var retryAfterType = headers[RetryAfterHeader]?[SchemaProperty]?["type"]?.GetValue<string>();
		if (occurrences != 1 || !string.Equals(retryAfterType, "string", StringComparison.Ordinal))
		{
			throw new InvalidDataException($"Unexpected Retry-After contract for GET {path} status {status}.");
		}
	}

	private static JsonObject CreateProjection(JsonObject source)
	{
		var schemas = JsonNode.Parse("""
		{
			"CatalogReference": {
				"type": "object",
				"additionalProperties": false,
				"properties": {
					"name": { "type": "string", "nullable": true }
				}
			},
			"MediaData": {
				"type": "object",
				"additionalProperties": false,
				"properties": {
					"themes": {
						"type": "array",
						"nullable": true,
						"items": { "$ref": "#/components/schemas/CatalogReference" }
					},
					"demographics": {
						"type": "array",
						"nullable": true,
						"items": { "$ref": "#/components/schemas/CatalogReference" }
					}
				}
			},
			"MediaResponse": {
				"type": "object",
				"additionalProperties": false,
				"required": ["data"],
				"properties": {
					"data": { "$ref": "#/components/schemas/MediaData" }
				}
			},
			"PersonReference": {
				"type": "object",
				"additionalProperties": false,
				"properties": {
					"name": { "type": "string", "nullable": true },
					"url": { "type": "string", "nullable": true }
				}
			},
			"VoiceActor": {
				"type": "object",
				"additionalProperties": false,
				"properties": {
					"person": { "$ref": "#/components/schemas/PersonReference" },
					"language": { "type": "string", "nullable": true }
				}
			},
			"Character": {
				"type": "object",
				"additionalProperties": false,
				"properties": {
					"voice_actors": {
						"type": "array",
						"nullable": true,
						"items": { "$ref": "#/components/schemas/VoiceActor" }
					}
				}
			},
			"CharactersResponse": {
				"type": "object",
				"additionalProperties": false,
				"required": ["data"],
				"properties": {
					"data": {
						"type": "array",
						"items": { "$ref": "#/components/schemas/Character" }
					}
				}
			},
			"TenraiError": {
				"type": "object",
				"additionalProperties": false,
				"properties": {
					"status": { "type": "integer", "format": "int32", "nullable": true },
					"type": { "type": "string", "nullable": true },
					"message": { "type": "string", "nullable": true },
					"error": { "type": "string", "nullable": true },
					"path": { "type": "string", "nullable": true }
				}
			}
		}
		""")?.AsObject() ?? throw new InvalidDataException("The projected schemas are invalid.");

		var paths = new JsonObject();
		foreach (var contract in Operations)
		{
			var originalResponses = source["paths"]?[contract.Path]?["get"]?["responses"]?.AsObject()
				?? throw new InvalidDataException($"Missing responses for GET {contract.Path}.");
			var responses = new JsonObject
			{
				["200"] = CreateResponse(originalResponses["200"], contract.SuccessSchema, includeRetryAfter: false),
			};
			foreach (var status in ErrorStatuses)
			{
				responses[status] = CreateResponse(
					originalResponses[status],
					"TenraiError",
					string.Equals(status, "429", StringComparison.Ordinal) || string.Equals(status, "503", StringComparison.Ordinal));
			}

			paths[contract.Path] = new JsonObject
			{
				["get"] = new JsonObject
				{
					["operationId"] = contract.OperationId,
					["tags"] = new JsonArray(JsonValue.Create("Tenrai")),
					["parameters"] = new JsonArray(
						new JsonObject
						{
							["name"] = "id",
							["in"] = "path",
							["required"] = true,
							[SchemaProperty] = new JsonObject
							{
								["type"] = "integer",
								["format"] = "int32",
							},
						}),
					["responses"] = responses,
				},
			};
		}

		return new JsonObject
		{
			["openapi"] = "3.0.3",
			["info"] = new JsonObject
			{
				["title"] = "Tantei projected Tenrai contract",
				["version"] = GetRequiredString(source["info"]?.AsObject(), "version", "the API version"),
			},
			["servers"] = new JsonArray(new JsonObject { ["url"] = "https://api.tenrai.org/v1" }),
			["paths"] = paths,
			["components"] = new JsonObject { ["schemas"] = schemas },
		};
	}

	private static JsonObject CreateResponse(JsonNode? original, string schemaName, bool includeRetryAfter)
	{
		var originalObject = original?.AsObject() ?? throw new InvalidDataException("Missing source response.");
		var response = new JsonObject
		{
			["description"] = GetRequiredString(originalObject, "description", "a response description"),
			["content"] = new JsonObject
			{
				["application/json"] = new JsonObject
				{
					[SchemaProperty] = new JsonObject { ["$ref"] = $"#/components/schemas/{schemaName}" },
				},
			},
		};
		if (includeRetryAfter)
		{
			response["headers"] = new JsonObject
			{
				[RetryAfterHeader] = originalObject["headers"]?[RetryAfterHeader]?.DeepClone()
					?? throw new InvalidDataException($"Missing {RetryAfterHeader} header."),
			};
		}

		return response;
	}

	private static string GetRequiredString(JsonObject? parent, string propertyName, string description)
	{
		var value = parent?[propertyName]?.GetValue<string>();
		return string.IsNullOrWhiteSpace(value) ? throw new InvalidDataException($"Missing {description}.") : value;
	}

	private static void ValidateNoDuplicateProperties(JsonElement element, string location)
	{
		if (element.ValueKind == JsonValueKind.Object)
		{
			var propertyNames = new HashSet<string>(StringComparer.Ordinal);
			foreach (var property in element.EnumerateObject())
			{
				if (!propertyNames.Add(property.Name))
				{
					throw new InvalidDataException($"Duplicate JSON property '{property.Name}' at {location}.");
				}

				ValidateNoDuplicateProperties(property.Value, $"{location}.{property.Name}");
			}

			return;
		}

		if (element.ValueKind == JsonValueKind.Array)
		{
			var index = 0;
			foreach (var item in element.EnumerateArray())
			{
				ValidateNoDuplicateProperties(item, $"{location}[{index.ToString(CultureInfo.InvariantCulture)}]");
				index++;
			}
		}
	}

	private sealed record OperationContract(string Path, string OperationId, string SuccessSchema);
}
