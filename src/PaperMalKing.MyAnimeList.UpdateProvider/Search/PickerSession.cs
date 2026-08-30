// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

[SuppressMessage(
	"Design",
	"CA1001:Types that own disposable fields should be disposable",
	Justification = "The cache eviction callback stops owned timers while the per-session gate may still be held")]
internal sealed class PickerSession(
	string searchId,
	PickerSnapshot snapshot,
	PickerSearchContext context,
	IPickerMessageTarget target,
	PickerSessionStore store,
	ILogger logger)
{
	public static readonly TimeSpan InactivityLifetime = TimeSpan.FromSeconds(90);
	public static readonly TimeSpan AbsoluteLifetime = TimeSpan.FromMinutes(14);
	private readonly PickerSnapshot _snapshot = snapshot;
	private readonly PickerSearchContext _context = context;
	private readonly IPickerMessageTarget _target = target;
	private readonly PickerSessionStore _store = store;
	private readonly ILogger _logger = logger;
	private readonly SemaphoreSlim _gate = new(1, 1);
	private ITimer? _inactivityTimer;
	private ITimer? _absoluteTimer;
	private PickerTerminalReason? _terminalReason;
	private int _page;
	private bool _disposed;

	public string SearchId { get; } = searchId;

	public PickerView InitialView => PickerRenderer.Render(this._snapshot, this.SearchId, this._page);

	public bool IsRequester(ulong discordUserId) => this._context.DiscordUserId == discordUserId;

	public void Start(TimeProvider timeProvider)
	{
		this._inactivityTimer = timeProvider.CreateTimer(
			static state => ((PickerSession)state!).BeginExpiry(PickerTerminalReason.InactivityTimeout),
			this,
			InactivityLifetime,
			Timeout.InfiniteTimeSpan);
		var absoluteDelay = this._context.InvokedAt + AbsoluteLifetime - timeProvider.GetUtcNow();
		this._absoluteTimer = timeProvider.CreateTimer(
			static state => ((PickerSession)state!).BeginExpiry(PickerTerminalReason.AbsoluteTimeout),
			this,
			absoluteDelay > TimeSpan.Zero ? absoluteDelay : TimeSpan.Zero,
			Timeout.InfiniteTimeSpan);
	}

	public async Task HandleAsync(PickerCustomId customId, IPickerInteraction interaction)
	{
		await this._gate.WaitAsync().ConfigureAwait(false);
		try
		{
			using var scope = this.BeginScope();
			if (this._terminalReason.HasValue)
			{
				return;
			}

			this._inactivityTimer?.Change(InactivityLifetime, Timeout.InfiniteTimeSpan);
			switch (customId.Action)
			{
				case PickerAction.Previous:
					this._page = Math.Max(0, this._page - 1);
					await interaction.EditAsync(PickerRenderer.Render(this._snapshot, this.SearchId, this._page)).ConfigureAwait(false);
					break;
				case PickerAction.Page:
					await interaction.EditAsync(PickerRenderer.Render(this._snapshot, this.SearchId, this._page)).ConfigureAwait(false);
					break;
				case PickerAction.Next:
					this._page = Math.Min(this._snapshot.PageCount - 1, this._page + 1);
					await interaction.EditAsync(PickerRenderer.Render(this._snapshot, this.SearchId, this._page)).ConfigureAwait(false);
					break;
				case PickerAction.Cancel:
					this.End(PickerTerminalReason.Cancelled);
					await this.TryPushAsync(() => interaction.EditAsync(PickerView.Terminal(SearchMessages.Cancelled))).ConfigureAwait(false);
					break;
				case PickerAction.Pick:
					await this.PickAsync(interaction).ConfigureAwait(false);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(customId));
			}
		}
		finally
		{
			this._gate.Release();
		}
	}

	public async Task HandleUnexpectedFailureAsync(IPickerInteraction interaction, Exception exception)
	{
		await this._gate.WaitAsync().ConfigureAwait(false);
		try
		{
			using var scope = this.BeginScope();
			if (this._terminalReason.HasValue)
			{
				return;
			}

			this._logger.PickerInteractionFailed(exception);
			this.End(PickerTerminalReason.InteractionFailed);
			var view = PickerView.Terminal(SearchMessages.Unexpected);
			await this.TryPushAsync(() => interaction.HasAcknowledged ? interaction.EditAsync(view) : interaction.UpdateAsync(view))
					  .ConfigureAwait(false);
		}
		finally
		{
			this._gate.Release();
		}
	}

	public void Stop()
	{
		if (this._disposed)
		{
			return;
		}

		this._disposed = true;
		this._inactivityTimer?.Dispose();
		this._absoluteTimer?.Dispose();
	}

	private async Task PickAsync(IPickerInteraction interaction)
	{
		if (interaction.Values.Count != 1 ||
			!int.TryParse(interaction.Values[0], NumberStyles.None, CultureInfo.InvariantCulture, out var selectedIndex) ||
			(uint)selectedIndex >= (uint)this._snapshot.Results.Count)
		{
			throw new FormatException("The Picker selection is invalid.");
		}

		var result = this._snapshot.Results[selectedIndex];
		try
		{
			await this._target.SendPublicAsync(result.BuildEmbed(this._context)).ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			if (SearchPostFailure.IsForbidden(exception))
			{
				this._logger.PublicPostForbidden();
			}
			else
			{
				this._logger.PublicPostFailed(exception);
			}

			this.End(PickerTerminalReason.PostFailed);
			await this.TryPushAsync(() => this._target.EditOriginalAsync(PickerView.Terminal(SearchMessages.PostFailed))).ConfigureAwait(false);
			return;
		}

		this.End(PickerTerminalReason.Picked, result.Id);
		await this.TryPushAsync(this._target.DeleteOriginalAsync).ConfigureAwait(false);
	}

	private void BeginExpiry(PickerTerminalReason reason)
	{
		_ = this.ExpireAsync(reason);
	}

	private async Task ExpireAsync(PickerTerminalReason reason)
	{
		await this._gate.WaitAsync().ConfigureAwait(false);
		try
		{
			using var scope = this.BeginScope();
			if (this._terminalReason.HasValue)
			{
				return;
			}

			this.End(reason);
			var content = reason == PickerTerminalReason.InactivityTimeout
				? SearchMessages.IdledOut
				: SearchMessages.Expired;
			await this.TryPushAsync(() => this._target.EditOriginalAsync(PickerView.Terminal(content))).ConfigureAwait(false);
		}
		finally
		{
			this._gate.Release();
		}
	}

	private async Task TryPushAsync(Func<Task> push)
	{
		try
		{
			await push().ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			this._logger.TerminalStatePushFailed(exception);
		}
	}

	private void End(PickerTerminalReason reason, uint? selectedMediaId = null)
	{
		this._terminalReason = reason;
		this._logger.PickerEnded(reason, selectedMediaId);
		this._store.End(this);
	}

	private IDisposable? BeginScope() => this._logger.SearchScope(this.SearchId, this._context);
}
