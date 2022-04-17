// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Common;

[AttributeUsage(AttributeTargets.Field)]
public sealed class FeatureDescriptionAttribute : Attribute
{
	public string Description { get; }

	public string Summary { get; }

	public FeatureDescriptionAttribute(string description, string summary)
	{
		this.Description = description ?? throw new ArgumentNullException(nameof(description));
		this.Summary = summary ?? throw new ArgumentNullException(nameof(summary));
	}
}