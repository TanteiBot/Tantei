// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Scoring;

public abstract class BaseScoreSystem<T> : BaseScoreSystem where T : notnull
{
	public T UserScore { get; }

	protected BaseScoreSystem(T userScore, string? display) : base(display)
	{
		this.UserScore = userScore;
	}
}