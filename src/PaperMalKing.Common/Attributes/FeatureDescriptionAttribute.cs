// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;

namespace PaperMalKing.Common.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class FeatureDescriptionAttribute : Attribute
{
	public FeatureDescriptionAttribute(string description, string summary)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(description);
		ArgumentException.ThrowIfNullOrWhiteSpace(summary);
		this.Description = description;
		this.Summary = summary;
	}

	public string Description { get; }

	public string Summary { get; }
}