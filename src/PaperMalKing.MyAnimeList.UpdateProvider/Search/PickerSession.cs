// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

[SuppressMessage(
	"Design",
	"CA1001:Types that own disposable fields should be disposable",
	Justification = "Session termination and cache eviction dispose the owned timers")]
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
	private readonly PickerSessionStore _store = store;
	private readonly ILogger _logger = logger;
	private readonly ulong _requesterDiscordUserId = context.DiscordUserId;
	private readonly Lock _lifecycleGate = new();
	private readonly SemaphoreSlim _interactionGate = new(1, 1);
	private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
	private readonly CancellationTokenSource _lifetimeCancellation = new();
	private PickerSnapshot? _snapshot = snapshot;
	private PickerSearchContext? _context = context;
	private IPickerMessageTarget? _target = target;
	private ITimer? _inactivityTimer;
	private ITimer? _absoluteTimer;
	private PickerSessionState _state;
	private int _page;

	public string SearchId { get; } = searchId;

	public DateTimeOffset ExpiresAt { get; } = context.InvokedAt + AbsoluteLifetime;

	public Task Completion => this._completion.Task;

	public CancellationToken LifetimeToken => this._lifetimeCancellation.Token;

	public bool IsRequester(ulong discordUserId) => this._requesterDiscordUserId == discordUserId;

	public void Start(TimeProvider timeProvider)
	{
		ArgumentNullException.ThrowIfNull(timeProvider);
		lock (this._lifecycleGate)
		{
			if (this._state != PickerSessionState.Pending)
			{
				return;
			}

			this._inactivityTimer = timeProvider.CreateTimer(
				static state => ((PickerSession)state!).BeginExpiry(PickerTerminalReason.InactivityTimeout),
				this,
				Timeout.InfiniteTimeSpan,
				Timeout.InfiniteTimeSpan);
			this._absoluteTimer = timeProvider.CreateTimer(
				static state => ((PickerSession)state!).BeginExpiry(PickerTerminalReason.AbsoluteTimeout),
				this,
				Timeout.InfiniteTimeSpan,
				Timeout.InfiniteTimeSpan);
			var absoluteDelay = this.ExpiresAt - timeProvider.GetUtcNow();
			_ = this._inactivityTimer.Change(InactivityLifetime, Timeout.InfiniteTimeSpan);
			_ = this._absoluteTimer.Change(
				absoluteDelay > TimeSpan.Zero ? absoluteDelay : TimeSpan.Zero,
				Timeout.InfiniteTimeSpan);
		}
	}

	public bool Activate(TimeProvider timeProvider)
	{
		ArgumentNullException.ThrowIfNull(timeProvider);
		var lifetime = this.ExpiresAt - timeProvider.GetUtcNow();
		if (lifetime <= TimeSpan.Zero)
		{
			this.BeginExpiry(PickerTerminalReason.AbsoluteTimeout);
			return false;
		}

		lock (this._lifecycleGate)
		{
			if (this._state != PickerSessionState.Pending)
			{
				return false;
			}

			this._state = PickerSessionState.Active;
			try
			{
				this._store.Add(this, lifetime);
			}
			catch
			{
				if (this._state == PickerSessionState.Active)
				{
					this._state = PickerSessionState.Failed;
					this.ReleaseResources().Release();
				}

				throw;
			}

			return this._state == PickerSessionState.Active;
		}
	}

	public void AbortActivation()
	{
		SessionResources resources;
		lock (this._lifecycleGate)
		{
			if (this._state != PickerSessionState.Pending)
			{
				return;
			}

			this._state = PickerSessionState.Failed;
			resources = this.ReleaseResources();
		}

		resources.Release();
	}

	public async Task HandleAsync(PickerCustomId customId, IPickerInteraction interaction)
	{
		await this._interactionGate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
		try
		{
			switch (customId.Action)
			{
				case PickerAction.Previous:
					await this.ChangePageAsync(interaction, -1).ConfigureAwait(false);
					break;
				case PickerAction.Page:
					await this.ChangePageAsync(interaction, 0).ConfigureAwait(false);
					break;
				case PickerAction.Next:
					await this.ChangePageAsync(interaction, 1).ConfigureAwait(false);
					break;
				case PickerAction.Cancel:
					await this.CancelAsync(interaction).ConfigureAwait(false);
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
			this._interactionGate.Release();
		}
	}

	public async Task HandleUnexpectedFailureAsync(IPickerInteraction interaction, Exception exception)
	{
		await this._interactionGate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
		try
		{
			var transition = this.TryEnd(allowPending: false, requirePosting: false);
			if (transition is null)
			{
				return;
			}

			using (this.BeginScope(transition.Value.Context))
			{
				this._logger.PickerInteractionFailed(exception);
			}

			this.CompleteEnd(transition.Value, PickerTerminalReason.InteractionFailed, selectedMediaId: null);
			var view = PickerView.Terminal(SearchMessages.Unexpected);
			await this.TryPushAsync(() => interaction.HasAcknowledged ? interaction.EditAsync(view, CancellationToken.None) : interaction.UpdateAsync(view))
				.ConfigureAwait(false);
		}
		finally
		{
			this._interactionGate.Release();
		}
	}

	public void OnEvicted()
	{
		SessionResources resources;
		lock (this._lifecycleGate)
		{
			if (this._state is PickerSessionState.Terminal or PickerSessionState.Failed or PickerSessionState.Evicted)
			{
				return;
			}

			this._state = PickerSessionState.Evicted;
			resources = this.ReleaseResources();
		}

		resources.Release();
	}

	private async Task ChangePageAsync(IPickerInteraction interaction, int offset)
	{
		PickerView view;
		CancellationToken cancellationToken;
		lock (this._lifecycleGate)
		{
			if (this._state != PickerSessionState.Active)
			{
				return;
			}

			this.ResetInactivityTimer();
			this._page = Math.Clamp(this._page + offset, 0, this._snapshot!.PageCount - 1);
			view = PickerRenderer.Render(this._snapshot, this.SearchId, this._page);
			cancellationToken = this._lifetimeCancellation.Token;
		}

		await interaction.EditAsync(view, cancellationToken).ConfigureAwait(false);
	}

	private async Task CancelAsync(IPickerInteraction interaction)
	{
		this.ResetInactivityIfActive();
		var transition = this.TryEnd(allowPending: false, requirePosting: false);
		if (transition is null)
		{
			return;
		}

		this.CompleteEnd(transition.Value, PickerTerminalReason.Cancelled, selectedMediaId: null);
		await this.TryPushAsync(() => interaction.EditAsync(PickerView.Terminal(SearchMessages.Cancelled), CancellationToken.None)).ConfigureAwait(false);
	}

	[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "Expiry or eviction can change the state between the two locks")]
	private async Task PickAsync(IPickerInteraction interaction)
	{
		PickOperation operation;
		lock (this._lifecycleGate)
		{
			if (this._state != PickerSessionState.Active)
			{
				return;
			}

			this.ResetInactivityTimer();
			if (interaction.Values.Count != 1 ||
				!int.TryParse(interaction.Values[0], NumberStyles.None, CultureInfo.InvariantCulture, out var selectedIndex) ||
				(uint)selectedIndex >= (uint)this._snapshot!.Results.Count)
			{
				throw new FormatException("The Picker selection is invalid.");
			}

			var result = this._snapshot.Results[selectedIndex];
			operation = new(this._target!, result.BuildEmbed(this._context!), result.Id, this._lifetimeCancellation.Token);
			this._state = PickerSessionState.Posting;
		}

		try
		{
			Task post;
			lock (this._lifecycleGate)
			{
				if (this._state != PickerSessionState.Posting)
				{
					return;
				}

				post = operation.Target.SendPublicAsync(operation.Embed, operation.CancellationToken);
			}

			await post.ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			var transition = this.TryEnd(allowPending: false, requirePosting: true);
			if (transition is null)
			{
				return;
			}

			using (this.BeginScope(transition.Value.Context))
			{
				if (SearchPostFailure.IsForbidden(exception))
				{
					this._logger.PublicPostForbidden();
				}
				else
				{
					this._logger.PublicPostFailed(exception);
				}
			}

			this.CompleteEnd(transition.Value, PickerTerminalReason.PostFailed, selectedMediaId: null);
			await this.TryPushAsync(() => operation.Target.EditOriginalAsync(PickerView.Terminal(SearchMessages.PostFailed), CancellationToken.None)).ConfigureAwait(false);
			return;
		}

		var completed = this.TryEnd(allowPending: false, requirePosting: true);
		if (completed is null)
		{
			return;
		}

		this.CompleteEnd(completed.Value, PickerTerminalReason.Picked, operation.SelectedMediaId);
		await this.TryPushAsync(() => operation.Target.DeleteOriginalAsync(CancellationToken.None)).ConfigureAwait(false);
	}

	private void BeginExpiry(PickerTerminalReason reason)
	{
		var transition = this.TryEnd(allowPending: true, requirePosting: false);
		if (transition is null)
		{
			return;
		}

		this.CompleteEnd(transition.Value, reason, selectedMediaId: null);
		if (!transition.Value.WasActive)
		{
			return;
		}

		var content = reason == PickerTerminalReason.InactivityTimeout
			? SearchMessages.IdledOut
			: SearchMessages.Expired;
		_ = this.TryPushAsync(() => transition.Value.Target.EditOriginalAsync(PickerView.Terminal(content), CancellationToken.None));
	}

	private TerminalTransition? TryEnd(bool allowPending, bool requirePosting)
	{
		lock (this._lifecycleGate)
		{
			var wasActive = this._state is PickerSessionState.Active or PickerSessionState.Posting;
			if ((!wasActive && !(allowPending && this._state == PickerSessionState.Pending)) ||
				(requirePosting && this._state != PickerSessionState.Posting))
			{
				return null;
			}

			this._state = PickerSessionState.Terminal;
			return new(this._target!, this._context!, wasActive, this.ReleaseResources());
		}
	}

	private void CompleteEnd(TerminalTransition transition, PickerTerminalReason reason, uint? selectedMediaId)
	{
		transition.Resources.Release();
		this._store.End(this);
		using var scope = this.BeginScope(transition.Context);
		this._logger.PickerEnded(reason, selectedMediaId);
	}

	private void ResetInactivityIfActive()
	{
		lock (this._lifecycleGate)
		{
			if (this._state == PickerSessionState.Active)
			{
				this.ResetInactivityTimer();
			}
		}
	}

	private void ResetInactivityTimer() => this._inactivityTimer?.Change(InactivityLifetime, Timeout.InfiniteTimeSpan);

	private SessionResources ReleaseResources()
	{
		var resources = new SessionResources(this._inactivityTimer, this._absoluteTimer, this._lifetimeCancellation);
		this._inactivityTimer = null;
		this._absoluteTimer = null;
		this._snapshot = null;
		this._context = null;
		this._target = null;
		this._completion.TrySetResult();
		return resources;
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

	private IDisposable? BeginScope(PickerSearchContext searchContext) => this._logger.SearchScope(this.SearchId, searchContext);

	private readonly record struct PickOperation(
		IPickerMessageTarget Target,
		DiscordEmbedBuilder Embed,
		uint SelectedMediaId,
		CancellationToken CancellationToken);

	private readonly record struct TerminalTransition(
		IPickerMessageTarget Target,
		PickerSearchContext Context,
		bool WasActive,
		SessionResources Resources);

	private readonly record struct SessionResources(
		ITimer? InactivityTimer,
		ITimer? AbsoluteTimer,
		CancellationTokenSource LifetimeCancellation)
	{
		public void Release()
		{
			this.LifetimeCancellation.Cancel();
			this.InactivityTimer?.Dispose();
			this.AbsoluteTimer?.Dispose();
			this.LifetimeCancellation.Dispose();
		}
	}

	private enum PickerSessionState
	{
		Pending = 0,
		Active = 1,
		Posting = 2,
		Terminal = 3,
		Failed = 4,
		Evicted = 5,
	}
}
