// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "The Picker owns every lifecycle transition")]
[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "The Picker owns every lifecycle transition")]
internal sealed class MalSearchPicker(IMemoryCache _cache, TimeProvider _timeProvider, ILogger<MalSearchPicker> _logger)
{
	private const string SessionPrefix = "mal-search-session:";
	private const string TerminalPrefix = "mal-search-terminal:";
	private static readonly TimeSpan InactivityLifetime = TimeSpan.FromSeconds(90);
	private static readonly TimeSpan AbsoluteLifetime = TimeSpan.FromMinutes(14);
	private static readonly TimeSpan TerminalMarkerLifetime = TimeSpan.FromMinutes(1);

	public Task OpenAsync(
		string searchId,
		IEnumerable<SearchResult> results,
		PickerSearchContext context,
		IPickerMessageTarget target)
	{
		ArgumentNullException.ThrowIfNull(results);
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(target);
		return this.OpenCoreAsync(searchId, results, context, target);
	}

	public Task<bool> HandleAsync(IPickerInteraction interaction)
	{
		ArgumentNullException.ThrowIfNull(interaction);
		return this.HandleCoreAsync(interaction);
	}

	private static IDisposable? BeginInteractionScope(ILogger logger, string searchId, IPickerInteraction interaction) =>
		logger.PickerInteractionScope(
			searchId,
			interaction.DiscordUserId,
			interaction.DiscordDisplayName,
			interaction.GuildId,
			interaction.ChannelId);

	private static string SessionKey(string searchId) => SessionPrefix + searchId;

	private static string TerminalKey(string searchId) => TerminalPrefix + searchId;

	private async Task OpenCoreAsync(
		string searchId,
		IEnumerable<SearchResult> results,
		PickerSearchContext context,
		IPickerMessageTarget target)
	{
		if (context.InvokedAt + AbsoluteLifetime <= _timeProvider.GetUtcNow())
		{
			await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.Expired)).ConfigureAwait(false);
			return;
		}

		var snapshot = PickerSnapshot.Create(results);
		var view = PickerRenderer.Render(snapshot, searchId, page: 0);
		var state = new PickerState(searchId, snapshot, context, target);
		try
		{
			this.Start(state);
			var delivery = target.EditOriginalAsync(view, state.LifetimeCancellation.Token);
			if (await Task.WhenAny(delivery, state.Completion).ConfigureAwait(false) != delivery)
			{
				_ = delivery.ContinueWith(
					static task => _ = task.Exception,
					CancellationToken.None,
					TaskContinuationOptions.OnlyOnFaulted,
					TaskScheduler.Default);
				return;
			}

			await delivery.ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (state.Completion.IsCompleted)
		{
			return;
		}
		catch
		{
			this.AbortOpening(state);
			throw;
		}

		_ = this.Activate(state);
	}

	private async Task<bool> HandleCoreAsync(IPickerInteraction interaction)
	{
		if (!PickerCustomId.HasPrefix(interaction.CustomId))
		{
			return false;
		}

		if (!PickerCustomId.TryParse(interaction.CustomId, out var customId))
		{
			var outcome = PickerInteractionOutcome.Replace(PickerView.Terminal(SearchMessages.Unavailable));
			await this.TryPushAsync(() => interaction.ApplyOutcomeAsync(outcome, CancellationToken.None)).ConfigureAwait(false);
			return true;
		}

		PickerState? state = null;
		try
		{
			var lookup = this.Find(customId.SearchId);
			state = lookup.State;
			if (lookup.Kind == PickerLookup.Absent)
			{
				using var scope = BeginInteractionScope(_logger, customId.SearchId, interaction);
				_logger.PickerUnavailable();
				var outcome = PickerInteractionOutcome.Replace(PickerView.Terminal(SearchMessages.Unavailable));
				await interaction.ApplyOutcomeAsync(outcome, CancellationToken.None).ConfigureAwait(false);
				return true;
			}

			if (lookup.Kind == PickerLookup.Terminal)
			{
				await interaction.ApplyOutcomeAsync(PickerInteractionOutcome.Recognized, CancellationToken.None).ConfigureAwait(false);
				return true;
			}

			if (state is null)
			{
				throw new InvalidOperationException("An active Picker lookup did not return its state.");
			}

			await interaction.ApplyOutcomeAsync(PickerInteractionOutcome.Recognized, CancellationToken.None).ConfigureAwait(false);
			if (state.RequesterDiscordUserId != interaction.DiscordUserId)
			{
				return true;
			}

			await this.HandleAsync(state, customId, interaction).ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			if (state is not null)
			{
				await this.HandleUnexpectedFailureAsync(state, interaction, exception).ConfigureAwait(false);
			}
			else
			{
				using var scope = BeginInteractionScope(_logger, customId.SearchId, interaction);
				_logger.PickerInteractionFailed(exception);
				await this.TryPushUnexpectedFailureAsync(interaction, CancellationToken.None).ConfigureAwait(false);
			}
		}

		return true;
	}

	private void Start(PickerState state)
	{
		lock (state.LifecycleGate)
		{
			if (state.Phase != PickerPhase.Opening)
			{
				return;
			}

			state.InactivityTimer = _timeProvider.CreateTimer(
				static timerState =>
				{
					var expiry = (ExpiryTimerState)timerState!;
					expiry.Picker.BeginExpiry(expiry.State, PickerTerminalReason.InactivityTimeout);
				},
				new ExpiryTimerState(this, state),
				Timeout.InfiniteTimeSpan,
				Timeout.InfiniteTimeSpan);
			state.AbsoluteTimer = _timeProvider.CreateTimer(
				static timerState =>
				{
					var expiry = (ExpiryTimerState)timerState!;
					expiry.Picker.BeginExpiry(expiry.State, PickerTerminalReason.AbsoluteTimeout);
				},
				new ExpiryTimerState(this, state),
				Timeout.InfiniteTimeSpan,
				Timeout.InfiniteTimeSpan);
			var absoluteDelay = state.ExpiresAt - _timeProvider.GetUtcNow();
			_ = state.InactivityTimer.Change(InactivityLifetime, Timeout.InfiniteTimeSpan);
			_ = state.AbsoluteTimer.Change(
				absoluteDelay > TimeSpan.Zero ? absoluteDelay : TimeSpan.Zero,
				Timeout.InfiniteTimeSpan);
		}
	}

	private bool Activate(PickerState state)
	{
		var lifetime = state.ExpiresAt - _timeProvider.GetUtcNow();
		if (lifetime <= TimeSpan.Zero)
		{
			this.BeginExpiry(state, PickerTerminalReason.AbsoluteTimeout);
			return false;
		}

		lock (state.LifecycleGate)
		{
			if (state.Phase != PickerPhase.Opening)
			{
				return false;
			}

			state.Phase = PickerPhase.Active;
			try
			{
				_cache.Set(SessionKey(state.SearchId), state, new MemoryCacheEntryOptions()
					.SetAbsoluteExpiration(lifetime)
					.RegisterPostEvictionCallback(
						static (_, value, _, owner) =>
						{
							if (value is PickerState state)
							{
								((MalSearchPicker)owner!).OnEvicted(state);
							}
						},
						this));
			}
			catch
			{
				if (state.Phase == PickerPhase.Active)
				{
					state.Phase = PickerPhase.Failed;
					this.ReleaseResources(state).Release();
				}

				throw;
			}

			return state.Phase == PickerPhase.Active;
		}
	}

	private void AbortOpening(PickerState state)
	{
		SessionResources resources;
		lock (state.LifecycleGate)
		{
			if (state.Phase != PickerPhase.Opening)
			{
				return;
			}

			state.Phase = PickerPhase.Failed;
			resources = this.ReleaseResources(state);
		}

		resources.Release();
	}

	private async Task HandleAsync(PickerState state, PickerCustomId customId, IPickerInteraction interaction)
	{
		await state.InteractionGate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
		try
		{
			switch (customId.Action)
			{
				case PickerAction.Previous:
					await this.ChangePageAsync(state, interaction, -1).ConfigureAwait(false);
					break;
				case PickerAction.Page:
					await this.ChangePageAsync(state, interaction, 0).ConfigureAwait(false);
					break;
				case PickerAction.Next:
					await this.ChangePageAsync(state, interaction, 1).ConfigureAwait(false);
					break;
				case PickerAction.Cancel:
					await this.CancelAsync(state, interaction).ConfigureAwait(false);
					break;
				case PickerAction.Pick:
					await this.PickAsync(state, interaction).ConfigureAwait(false);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(customId));
			}
		}
		finally
		{
			state.InteractionGate.Release();
		}
	}

	private async Task HandleUnexpectedFailureAsync(PickerState state, IPickerInteraction interaction, Exception exception)
	{
		await state.InteractionGate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
		try
		{
			var transition = this.TryEnd(state, allowOpening: false, requirePosting: false);
			if (transition is null)
			{
				return;
			}

			using (this.BeginScope(state.SearchId, transition.Value.Context))
			{
				_logger.PickerInteractionFailed(exception);
			}

			this.CompleteEnd(state, transition.Value, PickerTerminalReason.InteractionFailed, selectedMediaId: null);
			await this.TryPushUnexpectedFailureAsync(interaction, CancellationToken.None).ConfigureAwait(false);
		}
		finally
		{
			state.InteractionGate.Release();
		}
	}

	private void OnEvicted(PickerState state)
	{
		SessionResources resources;
		lock (state.LifecycleGate)
		{
			if (state.Phase is PickerPhase.Terminal or PickerPhase.Failed or PickerPhase.Evicted)
			{
				return;
			}

			state.Phase = PickerPhase.Evicted;
			resources = this.ReleaseResources(state);
		}

		resources.Release();
	}

	private async Task ChangePageAsync(PickerState state, IPickerInteraction interaction, int offset)
	{
		PickerView view;
		CancellationToken cancellationToken;
		lock (state.LifecycleGate)
		{
			if (state.Phase != PickerPhase.Active)
			{
				return;
			}

			this.ResetInactivityTimer(state);
			state.Page = Math.Clamp(state.Page + offset, 0, state.Snapshot!.PageCount - 1);
			view = PickerRenderer.Render(state.Snapshot, state.SearchId, state.Page);
			cancellationToken = state.LifetimeCancellation.Token;
		}

		await interaction.ApplyOutcomeAsync(PickerInteractionOutcome.Replace(view), cancellationToken).ConfigureAwait(false);
	}

	private async Task CancelAsync(PickerState state, IPickerInteraction interaction)
	{
		this.ResetInactivityIfActive(state);
		var transition = this.TryEnd(state, allowOpening: false, requirePosting: false);
		if (transition is null)
		{
			return;
		}

		this.CompleteEnd(state, transition.Value, PickerTerminalReason.Cancelled, selectedMediaId: null);
		var outcome = PickerInteractionOutcome.Replace(PickerView.Terminal(SearchMessages.Cancelled));
		await this.TryPushAsync(() => interaction.ApplyOutcomeAsync(outcome, CancellationToken.None)).ConfigureAwait(false);
	}

	[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "Expiry or eviction can change the state between the two locks")]
	private async Task PickAsync(PickerState state, IPickerInteraction interaction)
	{
		PickOperation operation;
		lock (state.LifecycleGate)
		{
			if (state.Phase != PickerPhase.Active)
			{
				return;
			}

			this.ResetInactivityTimer(state);
			if (interaction.Values.Count != 1 ||
				!int.TryParse(interaction.Values[0], NumberStyles.None, CultureInfo.InvariantCulture, out var selectedIndex) ||
				(uint)selectedIndex >= (uint)state.Snapshot!.Results.Count)
			{
				throw new FormatException("The Picker selection is invalid.");
			}

			var result = state.Snapshot.Results[selectedIndex];
			operation = new(state.Target!, result.BuildEmbed(state.Context!), result.Id, state.LifetimeCancellation.Token);
			state.Phase = PickerPhase.Posting;
		}

		try
		{
			Task post;
			lock (state.LifecycleGate)
			{
				if (state.Phase != PickerPhase.Posting)
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
			var transition = this.TryEnd(state, allowOpening: false, requirePosting: true);
			if (transition is null)
			{
				return;
			}

			using (this.BeginScope(state.SearchId, transition.Value.Context))
			{
				if (SearchPostFailure.IsForbidden(exception))
				{
					_logger.PublicPostForbidden();
				}
				else
				{
					_logger.PublicPostFailed(exception);
				}
			}

			this.CompleteEnd(state, transition.Value, PickerTerminalReason.PostFailed, selectedMediaId: null);
			await this.TryPushAsync(() => operation.Target.EditOriginalAsync(PickerView.Terminal(SearchMessages.PostFailed), CancellationToken.None))
				.ConfigureAwait(false);
			return;
		}

		var completed = this.TryEnd(state, allowOpening: false, requirePosting: true);
		if (completed is null)
		{
			return;
		}

		this.CompleteEnd(state, completed.Value, PickerTerminalReason.Picked, operation.SelectedMediaId);
		await this.TryPushAsync(() => operation.Target.DeleteOriginalAsync(CancellationToken.None)).ConfigureAwait(false);
	}

	private void BeginExpiry(PickerState state, PickerTerminalReason reason)
	{
		var transition = this.TryEnd(state, allowOpening: true, requirePosting: false);
		if (transition is null)
		{
			return;
		}

		this.CompleteEnd(state, transition.Value, reason, selectedMediaId: null);
		if (!transition.Value.WasActive)
		{
			return;
		}

		var content = reason == PickerTerminalReason.InactivityTimeout
			? SearchMessages.IdledOut
			: SearchMessages.Expired;
		_ = this.TryPushAsync(() => transition.Value.Target.EditOriginalAsync(PickerView.Terminal(content), CancellationToken.None));
	}

	private TerminalTransition? TryEnd(PickerState state, bool allowOpening, bool requirePosting)
	{
		lock (state.LifecycleGate)
		{
			var wasActive = state.Phase is PickerPhase.Active or PickerPhase.Posting;
			if ((!wasActive && !(allowOpening && state.Phase == PickerPhase.Opening)) ||
				(requirePosting && state.Phase != PickerPhase.Posting))
			{
				return null;
			}

			state.Phase = PickerPhase.Terminal;
			return new(state.Target!, state.Context!, wasActive, this.ReleaseResources(state));
		}
	}

	private void CompleteEnd(PickerState state, TerminalTransition transition, PickerTerminalReason reason, uint? selectedMediaId)
	{
		transition.Resources.Release();
		_cache.Set(TerminalKey(state.SearchId), value: true, absoluteExpirationRelativeToNow: TerminalMarkerLifetime);
		_cache.Remove(SessionKey(state.SearchId));
		using var scope = this.BeginScope(state.SearchId, transition.Context);
		_logger.PickerEnded(reason, selectedMediaId);
	}

	private PickerStateLookup Find(string searchId)
	{
		if (_cache.TryGetValue(SessionKey(searchId), out PickerState? state))
		{
			return new(PickerLookup.Active, state);
		}

		var kind = _cache.TryGetValue(TerminalKey(searchId), out _) ? PickerLookup.Terminal : PickerLookup.Absent;
		return new(kind, State: null);
	}

	private void ResetInactivityIfActive(PickerState state)
	{
		lock (state.LifecycleGate)
		{
			if (state.Phase == PickerPhase.Active)
			{
				this.ResetInactivityTimer(state);
			}
		}
	}

	private void ResetInactivityTimer(PickerState state) => state.InactivityTimer?.Change(InactivityLifetime, Timeout.InfiniteTimeSpan);

	private SessionResources ReleaseResources(PickerState state)
	{
		var resources = new SessionResources(state.InactivityTimer, state.AbsoluteTimer, state.LifetimeCancellation);
		state.InactivityTimer = null;
		state.AbsoluteTimer = null;
		state.Snapshot = null;
		state.Context = null;
		state.Target = null;
		state.Complete();
		return resources;
	}

	private Task TryPushUnexpectedFailureAsync(IPickerInteraction interaction, CancellationToken cancellationToken = default)
	{
		var outcome = PickerInteractionOutcome.Replace(PickerView.Terminal(SearchMessages.Unexpected));
		return this.TryPushAsync(() => interaction.ApplyOutcomeAsync(outcome, cancellationToken));
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
			_logger.TerminalStatePushFailed(exception);
		}
	}

	private IDisposable? BeginScope(string searchId, PickerSearchContext searchContext) => _logger.SearchScope(searchId, searchContext);

	[SuppressMessage(
		"Design",
		"CA1001:Types that own disposable fields should be disposable",
		Justification = "Picker termination and cache eviction dispose the owned resources")]
	private sealed class PickerState(
		string searchId,
		PickerSnapshot snapshot,
		PickerSearchContext context,
		IPickerMessageTarget target)
	{
		public string SearchId { get; } = searchId;

		public ulong RequesterDiscordUserId { get; } = context.DiscordUserId;

		public DateTimeOffset ExpiresAt { get; } = context.InvokedAt + AbsoluteLifetime;

		public Lock LifecycleGate { get; } = new();

		public SemaphoreSlim InteractionGate { get; } = new(1, 1);

		private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public Task Completion => this._completion.Task;

		public CancellationTokenSource LifetimeCancellation { get; } = new();

		public PickerSnapshot? Snapshot { get; set; } = snapshot;

		public PickerSearchContext? Context { get; set; } = context;

		public IPickerMessageTarget? Target { get; set; } = target;

		public ITimer? InactivityTimer { get; set; }

		public ITimer? AbsoluteTimer { get; set; }

		public PickerPhase Phase { get; set; }

		public int Page { get; set; }

		public void Complete() => this._completion.TrySetResult();
	}

	private sealed record ExpiryTimerState(MalSearchPicker Picker, PickerState State);

	private readonly record struct PickerStateLookup(PickerLookup Kind, PickerState? State);

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

	private enum PickerLookup
	{
		Absent = 0,
		Active = 1,
		Terminal = 2,
	}

	private enum PickerPhase
	{
		Opening = 0,
		Active = 1,
		Posting = 2,
		Terminal = 3,
		Failed = 4,
		Evicted = 5,
	}
}
