// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using PaperMalKing.Api.Contracts.Responses;

namespace PaperMalKing.Api;

internal sealed class CreditsProvider(IWebHostEnvironment _environment)
{
	private const string ClientLicensesFileName = "licenses.json";

	private const string ServerLicensesResourceName = "Tantei.ServerLicenses.json";

	private readonly Lazy<CreditsResponse> _credits = new(
		() => new CreditsResponse(ReadClientLicenses(_environment), ReadServerLicenses()),
		LazyThreadSafetyMode.ExecutionAndPublication);

	public CreditsResponse Credits => this._credits.Value;

	internal static LicenseResponse[] ReadServerLicenses()
	{
		using var stream = typeof(CreditsProvider).Assembly.GetManifestResourceStream(ServerLicensesResourceName);

		return stream is null ? [] : JsonSerializer.Deserialize(stream, CreditsJsonContext.Default.LicenseResponseArray) ?? [];
	}

	private static LicenseResponse[] ReadClientLicenses(IWebHostEnvironment environment)
	{
		var file = environment.WebRootFileProvider.GetFileInfo(ClientLicensesFileName);
		if (!file.Exists)
		{
			return [];
		}

		using var stream = file.CreateReadStream();
		var licenses = JsonSerializer.Deserialize(stream, CreditsJsonContext.Default.LicenseResponseArray) ?? [];

		return Array.ConvertAll(licenses, static license => license.Url is null && IsSpdxIdentifier(license.Identifier)
			? license with { Url = new Uri($"https://spdx.org/licenses/{license.Identifier}.html") }
			: license);
	}

	private static bool IsSpdxIdentifier([NotNullWhen(true)] string? identifier) =>
		!string.IsNullOrEmpty(identifier) && identifier.All(static character => char.IsAsciiLetterOrDigit(character) || character is '-' or '.');
}
