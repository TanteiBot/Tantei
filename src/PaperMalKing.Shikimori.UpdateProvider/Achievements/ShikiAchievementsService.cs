// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Collections.Frozen;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.Shikimori.UpdateProvider.Achievements;

internal sealed class ShikiAchievementsService
{
	private readonly ILogger<ShikiAchievementsService> _logger;
	private readonly FrozenDictionary<AchievementKey, ShikiAchievement> _achievements;

	public ShikiAchievementsService(NekoFileJson options, ILogger<ShikiAchievementsService> logger)
	{
		this._logger = logger;
		this._achievements = this.CreateFromOptionsValue(options);
	}

	private FrozenDictionary<AchievementKey, ShikiAchievement> CreateFromOptionsValue(NekoFileJson neko)
	{
		if (neko.HumanNames.Count is 0 || neko.Achievements.Count is 0)
		{
			this._logger.DidntFindAnyAchievements();
			return FrozenDictionary<AchievementKey, ShikiAchievement>.Empty;
		}

		this._logger.FoundAchievements(neko.Achievements.Count);

		return neko.Achievements.ToDictionary(
			item => new AchievementKey(item.neko_id, item.level),
			item => new ShikiAchievement(
				item.neko_id,
				item.level,
				new(Wrapper.Abstractions.Constants.BaseUrl + item.image, UriKind.Absolute),
				!string.IsNullOrWhiteSpace(item.border_color) ? new(item.border_color) : DiscordColor.None,
				item.title_ru,
				item.text_ru,
				item.title_en,
				item.text_en,
				neko.HumanNames.GetValueOrDefault(item.neko_id))).ToFrozenDictionary();
	}

	public ShikiAchievement? GetAchievementOrNull(string id, byte level) => this._achievements.GetValueOrDefault(new(id, level));

	public bool IsAnyAchievementInfoAvailable => this._achievements.Count > 0;

	private readonly record struct AchievementKey(string Id, byte Level);
}