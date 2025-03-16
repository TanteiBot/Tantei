// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Collections.Frozen;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PaperMalKing.Shikimori.UpdateProvider.Achievements;

internal sealed class ShikiAchievementsService : IDisposable
{
	private readonly ILogger<ShikiAchievementsService> _logger;
	private readonly IDisposable? _monitor;
	private FrozenDictionary<AchievementKey, ShikiAchievement> _achievements;

	public ShikiAchievementsService(IOptionsMonitor<NekoFileJson> options, ILogger<ShikiAchievementsService> logger)
	{
		this._logger = logger;
		this._achievements = this.CreateFromOptionsValue(options.CurrentValue);
		this._monitor = options.OnChange(neko => this._achievements = this.CreateFromOptionsValue(neko));
	}

	private FrozenDictionary<AchievementKey, ShikiAchievement> CreateFromOptionsValue(NekoFileJson neko)
	{
		if (neko.HumanNames is null || neko.Achievements is null)
		{
			this._logger.DidntFindAnyAchievements();
			return FrozenDictionary<AchievementKey, ShikiAchievement>.Empty;
		}

		return neko.Achievements.ToDictionary(
			item => new AchievementKey(item.neko_id, item.level),
			item => new ShikiAchievement(
				item.neko_id,
				item.level,
				new(Wrapper.Abstractions.Constants.BaseUrl + item.image, UriKind.Absolute),
				item.border_color.HasValue ? new(item.border_color.Value) : DiscordColor.None,
				item.title_ru,
				item.text_ru,
				item.title_en,
				item.text_en,
				neko.HumanNames.GetValueOrDefault(item.neko_id))).ToFrozenDictionary();
	}

	public ShikiAchievement? GetAchievementOrNull(string id, byte level) => this._achievements.GetValueOrDefault(new(id, level));

	private readonly record struct AchievementKey(string Id, byte Level);

	public void Dispose()
	{
		this._monitor?.Dispose();
	}
}