// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Startup.Web;

public static class LoginRedirects
{
	public const string DefaultReturnUrl = "/";

	public static string SanitizeReturnUrl(string? returnUrl)
	{
		if (string.IsNullOrEmpty(returnUrl) || returnUrl[0] != '/')
		{
			return DefaultReturnUrl;
		}

		return returnUrl.Length > 1 && (returnUrl[1] == '/' || returnUrl[1] == '\\') ? DefaultReturnUrl : returnUrl;
	}

	public static string ClassifyRemoteFailure(string? errorQueryValue, string? failureMessage)
	{
		if (string.Equals(errorQueryValue, "access_denied", StringComparison.Ordinal))
		{
			return "cancelled";
		}

		return failureMessage?.Contains("Correlation failed", StringComparison.Ordinal) == true ? "expired" : "failed";
	}
}
