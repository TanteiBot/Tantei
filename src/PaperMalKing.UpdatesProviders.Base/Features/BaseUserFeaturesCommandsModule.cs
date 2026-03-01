// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.SlashCommands;
using Humanizer;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Features;

public abstract class BaseUserFeaturesCommandsModule<TUser, TFeature>
	(BaseUserFeaturesService<TUser, TFeature> userFeaturesService, ILogger<BaseUserFeaturesCommandsModule<TUser, TFeature>> logger) : BotCommandsModule
	where TUser : class, IUpdateProviderUser<TFeature>
	where TFeature : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	protected override bool IsResponseVisibleOnlyForRequester => true;

	public virtual async Task EnableFeatureCommand(InteractionContext context, string unparsedFeature)
	{
		using var scope = CreateLoggerScope(logger, context);
		var feature = FeaturesHelper<TFeature>.Parse(unparsedFeature);

		logger.TryingToEnableFeature(feature, context.Member!.DisplayName);
		try
		{
			await userFeaturesService.EnableFeaturesAsync(feature, context.User.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is UserFeaturesException ufe
				? EmbedTemplate.ErrorEmbed(ufe.GetFullMessage(), $"Failed enabling {feature.Humanize()}").Build()
				: EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			logger.FailedToEnableFeature(ex, feature, context.Member.DisplayName);
			throw;
		}

		logger.SuccessfullyEnabledFeature(feature, context.Member.DisplayName);
		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully enabled {feature.Humanize()} for you"));
	}

	public virtual async Task DisableFeatureCommand(InteractionContext context, string unparsedFeature)
	{
		using var scope = CreateLoggerScope(logger, context);

		var feature = FeaturesHelper<TFeature>.Parse(unparsedFeature);
		logger.TryingToDisableFeature(feature, context.Member!.DisplayName);
		try
		{
			await userFeaturesService.DisableFeaturesAsync(feature, context.User.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is UserFeaturesException ufe
				? EmbedTemplate.ErrorEmbed(ufe.GetFullMessage(), $"Failed disabling {feature.Humanize()}").Build()
				: EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			logger.FailedToDisableFeature(ex, feature, context.Member.DisplayName);
			throw;
		}

		logger.SuccessfullyDisabledFeature(feature, context.Member.DisplayName);
		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully disabled {feature.Humanize()} for you"));
	}

	public virtual async Task ListFeaturesCommand(InteractionContext context)
	{
		using var scope = CreateLoggerScope(logger, context);

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("All features")
															 .WithDescription(FeaturesHelper<TFeature>.Features
																 .Select(x => $"[{x.Description}] - {x.Summary}").JoinToString(";\n")));
	}

	public virtual async Task EnabledFeaturesCommand(InteractionContext context)
	{
		using var scope = CreateLoggerScope(logger, context);

		var featuresDesc = userFeaturesService.EnabledFeatures(context.User.Id);
		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("Your enabled features").WithDescription(featuresDesc));
	}
}