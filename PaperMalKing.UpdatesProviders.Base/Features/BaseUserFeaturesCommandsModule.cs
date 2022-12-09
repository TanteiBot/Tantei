// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Humanizer;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Features;

[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
public abstract class BaseUserFeaturesCommandsModule<T> : BaseCommandModule where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	protected IUserFeaturesService<T> UserFeaturesService { get; }
	protected ILogger<BaseUserFeaturesCommandsModule<T>> Logger { get; }

	protected BaseUserFeaturesCommandsModule(IUserFeaturesService<T> userFeaturesService, ILogger<BaseUserFeaturesCommandsModule<T>> logger)
	{
		this.UserFeaturesService = userFeaturesService;
		this.Logger = logger;
	}

	public virtual async Task EnableFeatureCommand(CommandContext context, params T[] features)
	{
		if (!features.Any())
			return;
		this.Logger.LogInformation("Trying to enable {Features} feature for {Username}", features!, context.Member!.DisplayName);
		try
		{
			await this.UserFeaturesService.EnableFeaturesAsync(features, context.User.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is UserFeaturesException ufe
				? EmbedTemplate.ErrorEmbed(context, ufe.Message, $"Failed enabling {features.Humanize().ToLowerInvariant()}")
				: EmbedTemplate.UnknownErrorEmbed(context);
			await context.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
			this.Logger.LogError(ex, "Failed to enable {Features} for {Username}", features, context.Member.DisplayName);
			throw;
		}

		this.Logger.LogInformation("Successfully enabled {Features} feature for {Username}", features, context.Member.DisplayName);
		await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context,
			$"Successfully enabled {features.Humanize()} for you")).ConfigureAwait(false);
	}

	public virtual async Task DisableFeatureCommand(CommandContext context, params T[] features)
	{
		if (!features.Any())
			return;
		this.Logger.LogInformation("Trying to disable {Features} feature for {Username}", features!, context.Member!.DisplayName);
		try
		{
			await this.UserFeaturesService.DisableFeaturesAsync(features, context.User.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is UserFeaturesException ufe
				? EmbedTemplate.ErrorEmbed(context, ufe.Message, $"Failed disabling {features.Humanize().ToLowerInvariant()}")
				: EmbedTemplate.UnknownErrorEmbed(context);
			await context.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
			this.Logger.LogError(ex, "Failed to disable {Features} for {Username}", features, context.Member.DisplayName);
			throw;
		}

		this.Logger.LogInformation("Successfully disabled {Features} feature for {Username}", features, context.Member.DisplayName);
		await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context,
			$"Successfully disabled {features.Humanize()} for you")).ConfigureAwait(false);
	}

	public virtual Task ListFeaturesCommand(CommandContext context) =>
		context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context, "All features")
												 .WithDescription(string.Join(";\n",
													 this.UserFeaturesService.Descriptions.Values
														 .Select(tuple => $"[{tuple.Item1}] - {tuple.Item2}"))));

	public virtual async Task EnabledFeaturesCommand(CommandContext context)
	{
		var featuresDesc = await this.UserFeaturesService.EnabledFeaturesAsync(context.User.Id).ConfigureAwait(false);
		await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context, "Your enabled features").WithDescription(featuresDesc)).ConfigureAwait(false);
	}
}