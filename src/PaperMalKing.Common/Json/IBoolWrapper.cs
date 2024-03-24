// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

namespace PaperMalKing.Common.Json;

public interface IBoolWrapper<out T>
	where T : IBoolWrapper<T>
{
	static abstract T TrueValue { get; }

	static abstract T FalseValue { get; }
}