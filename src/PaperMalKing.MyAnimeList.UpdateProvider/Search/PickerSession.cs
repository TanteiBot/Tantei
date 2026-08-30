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
					await interaction.EditAsync(PickerView.Terminal("Search cancelled.")).ConfigureAwait(false);
					this.End(PickerTerminalReason.Cancelled);
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
			if (this._terminalReason.HasValue)
			{
				return;
			}

			this._logger.UnexpectedInteractionFailure(exception, this.SearchId);
			this.End(PickerTerminalReason.InteractionFailed);
			var view = PickerView.Terminal("Something went wrong with this search. Run the command again.");
			try
			{
				if (interaction.HasAcknowledged)
				{
					await interaction.EditAsync(view).ConfigureAwait(false);
				}
				else
				{
					await interaction.UpdateAsync(view).ConfigureAwait(false);
				}
			}
#pragma warning disable CA1031
			catch (Exception pushException)
#pragma warning restore CA1031
			{
				this._logger.TerminalStatePushFailed(pushException, this.SearchId);
			}
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
			this._logger.SelectionPostFailed(exception, this.SearchId);
			this.End(PickerTerminalReason.PostFailed);
			try
			{
				await this._target.EditOriginalAsync(PickerView.Terminal("I couldn't post that result. Check my channel permissions and try again.")).ConfigureAwait(false);
			}
#pragma warning disable CA1031
			catch (Exception pushException)
#pragma warning restore CA1031
			{
				this._logger.TerminalStatePushFailed(pushException, this.SearchId);
			}

			return;
		}

		await this._target.DeleteOriginalAsync().ConfigureAwait(false);
		this.End(PickerTerminalReason.Picked);
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
			if (this._terminalReason.HasValue)
			{
				return;
			}

			this.End(reason);
			var content = reason == PickerTerminalReason.InactivityTimeout
				? "This search idled out. Run the command again."
				: "This search has expired. Run the command again.";
			try
			{
				await this._target.EditOriginalAsync(PickerView.Terminal(content)).ConfigureAwait(false);
			}
#pragma warning disable CA1031
			catch (Exception exception)
#pragma warning restore CA1031
			{
				this._logger.TerminalStatePushFailed(exception, this.SearchId);
			}
		}
		finally
		{
			this._gate.Release();
		}
	}

	private void End(PickerTerminalReason reason)
	{
		this._terminalReason = reason;
		this._store.End(this);
	}
}
