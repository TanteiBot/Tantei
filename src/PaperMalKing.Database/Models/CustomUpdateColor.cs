// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

namespace PaperMalKing.Database.Models;

public sealed class CustomUpdateColor
{
	public required byte UpdateType { get; init; }

	public required int ColorValue { get; init; }
}