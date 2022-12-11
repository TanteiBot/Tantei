// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Humanizer;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Features;

[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
public abstract class BaseUserFeaturesCommandsModule<T> : ApplicationCommandModule where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	protected IUserFeaturesService<T> UserFeaturesService { get; }
	protected ILogger<BaseUserFeaturesCommandsModule<T>> Logger { get; }

	protected BaseUserFeaturesCommandsModule(IUserFeaturesService<T> userFeaturesService, ILogger<BaseUserFeaturesCommandsModule<T>> logger)
	{
		this.UserFeaturesService = userFeaturesService;
		this.Logger = logger;
	}

	public virtual async Task EnableFeatureCommand(InteractionContext context, string unparsedFeature)
	{
		var feature = FeaturesHelper<T>.Parse(unparsedFeature);

		this.Logger.LogInformation("Trying to enable {Features} feature for {Username}", feature, context.Member!.DisplayName);
		try
		{
			await this.UserFeaturesService.EnableFeaturesAsync(feature, context.User.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is UserFeaturesException ufe
				? EmbedTemplate.ErrorEmbed(context, ufe.Message, $"Failed enabling {feature.Humanize().ToLowerInvariant()}")
				: EmbedTemplate.UnknownErrorEmbed(context);
			await context.CreateResponseAsync(embed: embed.Build()).ConfigureAwait(false);
			this.Logger.LogError(ex, "Failed to enable {Features} for {Username}", feature, context.Member.DisplayName);
			throw;
		}

		this.Logger.LogInformation("Successfully enabled {Features} feature for {Username}", feature, context.Member.DisplayName);
		await context.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, $"Successfully enabled {feature.Humanize()} for you"))
					 .ConfigureAwait(false);
	}

	public virtual async Task DisableFeatureCommand(InteractionContext context, string unparsedFeature)
	{
		var feature = FeaturesHelper<T>.Parse(unparsedFeature);
		this.Logger.LogInformation("Trying to disable {Features} feature for {Username}", feature, context.Member!.DisplayName);
		try
		{
			await this.UserFeaturesService.DisableFeaturesAsync(feature, context.User.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is UserFeaturesException ufe
				? EmbedTemplate.ErrorEmbed(context, ufe.Message, $"Failed disabling {feature.Humanize().ToLowerInvariant()}")
				: EmbedTemplate.UnknownErrorEmbed(context);
			await context.CreateResponseAsync(embed: embed.Build()).ConfigureAwait(false);
			this.Logger.LogError(ex, "Failed to disable {Features} for {Username}", feature, context.Member.DisplayName);
			throw;
		}

		this.Logger.LogInformation("Successfully disabled {Features} feature for {Username}", feature, context.Member.DisplayName);
		await context.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, $"Successfully disabled {feature.Humanize()} for you"))
					 .ConfigureAwait(false);
	}

	public virtual Task ListFeaturesCommand(InteractionContext context) => context.CreateResponseAsync(embed: EmbedTemplate
		.SuccessEmbed(context, "All features")
		.WithDescription(string.Join(";\n", FeaturesHelper<T>.FeaturesInfo.Values.Select(x => $"[{x.Description}] - {x.Summary}"))));

	public virtual async Task EnabledFeaturesCommand(InteractionContext context)
	{
		var featuresDesc = await this.UserFeaturesService.EnabledFeaturesAsync(context.User.Id).ConfigureAwait(false);
		await context.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, "Your enabled features").WithDescription(featuresDesc))
					 .ConfigureAwait(false);
	}
}