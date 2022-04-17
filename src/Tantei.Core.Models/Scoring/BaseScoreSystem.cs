// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Scoring;

public abstract class BaseScoreSystem
{
	public string? DisplayString { get; }

	protected BaseScoreSystem(string? displayString)
	{
		this.DisplayString = displayString;
	}
}