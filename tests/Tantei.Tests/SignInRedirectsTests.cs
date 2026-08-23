// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Startup.Web;

namespace Tantei.Tests;

public sealed class SignInRedirectsTests
{
	[Test]
	[Arguments("/guilds", "/guilds")]
	[Arguments("/guilds?tab=invitable", "/guilds?tab=invitable")]
	[Arguments("/", "/")]
	public async Task LocalPathsArePreserved(string input, string expected)
	{
		await Assert.That(SignInRedirects.SanitizeReturnUrl(input)).IsEqualTo(expected);
	}

	[Test]
	[Arguments("https://evil.example/steal")]
	[Arguments("//evil.example/steal")]
	[Arguments("/\\evil.example")]
	[Arguments("javascript:alert(1)")]
	[Arguments("")]
	[Arguments(null)]
	public async Task NonLocalUrlsFallBackToRoot(string? input)
	{
		await Assert.That(SignInRedirects.SanitizeReturnUrl(input)).IsEqualTo("/");
	}

	[Test]
	[Arguments("/guilds\r\nSet-Cookie: evil=1")]
	[Arguments("/guilds\nLocation: https://evil.example")]
	[Arguments("/guilds\0")]
	public async Task ControlCharactersFallBackToRoot(string input)
	{
		await Assert.That(SignInRedirects.SanitizeReturnUrl(input)).IsEqualTo("/");
	}

	[Test]
	public async Task AccessDeniedIsClassifiedAsCancelled()
	{
		await Assert.That(SignInRedirects.ClassifyRemoteFailure("access_denied", failureMessage: null)).IsEqualTo("cancelled");
	}

	[Test]
	public async Task CorrelationFailureIsClassifiedAsExpired()
	{
		await Assert.That(SignInRedirects.ClassifyRemoteFailure(errorQueryValue: null, "Correlation failed.")).IsEqualTo("expired");
	}

	[Test]
	public async Task UnknownFailureIsClassifiedAsFailed()
	{
		await Assert.That(SignInRedirects.ClassifyRemoteFailure(errorQueryValue: null, "Something exploded")).IsEqualTo("failed");
	}

	[Test]
	public async Task ClassificationNeverEchoesTheFailureMessage()
	{
		var classification = SignInRedirects.ClassifyRemoteFailure(errorQueryValue: null, "<script>alert(1)</script>");
		await Assert.That(classification).IsEqualTo("failed");
	}
}
