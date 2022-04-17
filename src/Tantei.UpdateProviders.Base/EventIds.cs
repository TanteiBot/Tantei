// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.UpdateProviders.Base;

internal static class EventIds
{
	public const int UpdateCheckStart = 0;

	public const int UpdateCheckError = 2;

	public const int UpdateCheckFinish = 3;

	public const int TryingToStartAlreadyStartedUpdateProvider = 4;

	public const int StartingUpdateProvider = 5;
}