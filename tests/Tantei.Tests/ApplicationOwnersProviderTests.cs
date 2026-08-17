// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Startup.Web;

namespace Tantei.Tests;

public sealed class ApplicationOwnersProviderTests
{
	private const ulong FirstOwnerId = 1UL;

	private const ulong SecondOwnerId = 2UL;

	private const ulong StrangerId = 3UL;

	private const int RepeatedChecks = 5;

	private sealed class FakeSource(params ulong[] owners) : IApplicationOwnersSource
	{
		public int CallCount { get; private set; }

		public Task<IReadOnlyCollection<ulong>> GetOwnerIdsAsync(CancellationToken cancellationToken)
		{
			this.CallCount++;
			return Task.FromResult<IReadOnlyCollection<ulong>>(owners);
		}
	}

	private sealed class ThrowingSource : IApplicationOwnersSource
	{
		public Task<IReadOnlyCollection<ulong>> GetOwnerIdsAsync(CancellationToken cancellationToken)
			=> throw new InvalidOperationException("no connection");
	}

	[Test]
	public async Task OwnerIsRecognised()
	{
		using var provider = new ApplicationOwnersProvider(new FakeSource(FirstOwnerId, SecondOwnerId), TimeProvider.System);

		await Assert.That(await provider.IsOwnerAsync(SecondOwnerId, TestContext.Current!.Execution.CancellationToken)).IsTrue();
	}

	[Test]
	public async Task NonOwnerIsRejected()
	{
		using var provider = new ApplicationOwnersProvider(new FakeSource(FirstOwnerId, SecondOwnerId), TimeProvider.System);

		await Assert.That(await provider.IsOwnerAsync(StrangerId, TestContext.Current!.Execution.CancellationToken)).IsFalse();
	}

	[Test]
	public async Task OwnersAreFetchedOnlyOnceAcrossManyChecks()
	{
		var source = new FakeSource(FirstOwnerId);
		using var provider = new ApplicationOwnersProvider(source, TimeProvider.System);

		for (var i = 0; i < RepeatedChecks; i++)
		{
			_ = await provider.IsOwnerAsync(FirstOwnerId, TestContext.Current!.Execution.CancellationToken);
		}

		await Assert.That(source.CallCount).IsEqualTo(1);
	}

	[Test]
	public async Task FailureToFetchOwnersDeniesRatherThanThrows()
	{
		using var provider = new ApplicationOwnersProvider(new ThrowingSource(), TimeProvider.System);

		await Assert.That(await provider.IsOwnerAsync(FirstOwnerId, TestContext.Current!.Execution.CancellationToken)).IsFalse();
	}
}
