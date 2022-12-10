// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.Database;

public sealed class DatabaseOptions
{
	public const string Database = nameof(Database);

	public required string ConnectionString { get; init; }
}