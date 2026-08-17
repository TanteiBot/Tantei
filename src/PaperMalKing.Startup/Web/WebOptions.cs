// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.ComponentModel.DataAnnotations;

namespace PaperMalKing.Startup.Web;

public sealed class WebOptions
{
	public const string Web = "Web";

	private const int MaxCookieLifetimeInDays = 365;

	private const int DefaultCookieLifetimeInDays = 30;

	[Range(1, MaxCookieLifetimeInDays)]
	public int CookieLifetimeInDays { get; init; } = DefaultCookieLifetimeInDays;

	public string? DataProtectionKeysDirectory { get; init; }
}
