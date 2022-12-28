// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;

namespace PaperMalKing.Common.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class FeatureDescriptionAttribute : Attribute
{
	public FeatureDescriptionAttribute(string description, string summary)
	{
		if (string.IsNullOrWhiteSpace(description))
			throw new ArgumentException(null, nameof(description));
		if (string.IsNullOrWhiteSpace(summary))
			throw new ArgumentException(null, nameof(summary));
		this.Description = description;
		this.Summary = summary;
	}

	public string Description { get; }

	public string Summary { get; }
}