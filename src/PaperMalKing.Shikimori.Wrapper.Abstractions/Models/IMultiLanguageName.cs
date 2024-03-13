// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public interface IMultiLanguageName
{
	string? Name { get; }

	string? RussianName { get; }
}