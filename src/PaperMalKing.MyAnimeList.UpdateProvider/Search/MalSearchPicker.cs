// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class MalSearchPicker(PickerSessionStore _store, TimeProvider _timeProvider, ILogger<MalSearchPicker> _logger)
{
	private const string UnavailableMessage = "This search is no longer available. Run the command again.";

	public PickerOpenResult OpenAnime(
		IEnumerable<RankedSearchResult<AnimeSearchResult>> results,
		PickerSearchContext context,
		IPickerMessageTarget target) => this.Open(PickerSnapshot.ForAnime(results), context, target);

	public PickerOpenResult OpenManga(
		IEnumerable<RankedSearchResult<MangaSearchResult>> results,
		PickerSearchContext context,
		IPickerMessageTarget target) => this.Open(PickerSnapshot.ForManga(results), context, target);

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

	private async Task<bool> HandleCoreAsync(IPickerInteraction interaction)
	{
		if (!PickerCustomId.HasPrefix(interaction.CustomId))
		{
			return false;
		}

		if (!PickerCustomId.TryParse(interaction.CustomId, out var customId))
		{
			await this.TryPushAsync(() => interaction.UpdateAsync(PickerView.Terminal(UnavailableMessage))).ConfigureAwait(false);
			return true;
		}

		PickerSession? session = null;
		try
		{
			var lookup = _store.Find(customId.SearchId);
			session = lookup.Session;
			if (lookup.Kind == PickerLookup.Absent)
			{
				using var scope = BeginInteractionScope(_logger, customId.SearchId, interaction);
				_logger.PickerUnavailable();
				await interaction.UpdateAsync(PickerView.Terminal(UnavailableMessage)).ConfigureAwait(false);
				return true;
			}

			if (lookup.Kind == PickerLookup.Terminal)
			{
				await interaction.DeferAsync().ConfigureAwait(false);
				return true;
			}

			if (session is null)
			{
				throw new InvalidOperationException("An active Picker lookup did not return a session.");
			}

			await interaction.DeferAsync().ConfigureAwait(false);
			if (!session.IsRequester(interaction.DiscordUserId))
			{
				return true;
			}

			await session.HandleAsync(customId, interaction).ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			if (session is not null)
			{
				await session.HandleUnexpectedFailureAsync(interaction, exception).ConfigureAwait(false);
			}
			else
			{
				using var scope = BeginInteractionScope(_logger, customId.SearchId, interaction);
				_logger.PickerInteractionFailed(exception);
				await this.TryPushUnexpectedFailureAsync(interaction).ConfigureAwait(false);
			}
		}

		return true;
	}

	private PickerOpenResult Open(PickerSnapshot snapshot, PickerSearchContext context, IPickerMessageTarget target)
	{
		ArgumentNullException.ThrowIfNull(context);
		ArgumentNullException.ThrowIfNull(target);
		var searchId = Guid.NewGuid().ToString("N");
		if (context.InvokedAt + PickerSession.AbsoluteLifetime <= _timeProvider.GetUtcNow())
		{
			return new(searchId, PickerView.Terminal("This search has expired. Run the command again."));
		}

		var session = new PickerSession(searchId, snapshot, context, target, _store, _logger);
		_store.Add(session);
		session.Start(_timeProvider);
		return new(searchId, session.InitialView);
	}

	private Task TryPushUnexpectedFailureAsync(IPickerInteraction interaction)
	{
		var view = PickerView.Terminal("Something went wrong with this search. Run the command again.");
		return this.TryPushAsync(() => interaction.HasAcknowledged ? interaction.EditAsync(view) : interaction.UpdateAsync(view));
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
}
