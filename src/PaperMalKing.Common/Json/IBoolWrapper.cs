// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

namespace PaperMalKing.Common.Json;

public interface IBoolWrapper<out T> where T: IBoolWrapper<T>
{
	abstract static T TrueValue { get; }

	abstract static T FalseValue { get; }
}