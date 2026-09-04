// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public interface ISearchMedia : IMultiLanguageName
{
	ulong Id { get; }

	string? EnglishName { get; }

	string? JapaneseName { get; }

	IReadOnlyList<string> Synonyms { get; }

	string? Kind { get; }

	float? Score { get; }

	string? Status { get; }

	int? Year { get; }

	string Url { get; }

	long Popularity { get; }

	bool IsAdult { get; }
}
