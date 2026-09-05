// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using GraphQLParser;
using GraphQLParser.Exceptions;

namespace Tantei.TestSupport;

public static class GraphQlAssertions
{
	public static void AssertValidGraphQl(string query)
	{
		ArgumentNullException.ThrowIfNull(query);
		try
		{
			_ = Parser.Parse(query);
		}
		catch (GraphQLSyntaxErrorException ex)
		{
			throw new InvalidOperationException($"Query is not syntactically valid GraphQL: {ex.Message}{Environment.NewLine}{query}", ex);
		}
	}
}
