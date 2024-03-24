// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Humanizer;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Features;

[SuppressMessage("Style", """VSTHRD200:Use "Async" suffix for async methods""", Justification = "This rule does not apply to commands")]
public abstract class BaseUserFeaturesCommandsModule<TUser, TFeature> : BotCommandsModule
	where TUser : class, IUpdateProviderUser<TFeature>
	where TFeature : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	protected BaseUserFeaturesService<TUser, TFeature> UserFeaturesService { get; }

	protected ILogger<BaseUserFeaturesCommandsModule<TUser, TFeature>> Logger { get; }

	protected BaseUserFeaturesCommandsModule(BaseUserFeaturesService<TUser, TFeature> userFeaturesService, ILogger<BaseUserFeaturesCommandsModule<TUser, TFeature>> logger)
	{
		this.UserFeaturesService = userFeaturesService;
		this.Logger = logger;
	}

	public virtual async Task EnableFeatureCommand(InteractionContext context, string unparsedFeature)
	{
		var feature = FeaturesHelper<TFeature>.Parse(unparsedFeature);

		this.Logger.TryingToEnableFeature(feature, context.Member!.DisplayName);
		try
		{
			await this.UserFeaturesService.EnableFeaturesAsync(feature, context.User.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is UserFeaturesException ufe
				? EmbedTemplate.ErrorEmbed(ufe.Message, $"Failed enabling {feature.Humanize()}").Build()
				: EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			this.Logger.FailedToEnableFeature(ex, feature, context.Member.DisplayName);
			throw;
		}

		this.Logger.SuccessfullyEnabledFeature(feature, context.Member.DisplayName);
		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully enabled {feature.Humanize()} for you"));
	}

	public virtual async Task DisableFeatureCommand(InteractionContext context, string unparsedFeature)
	{
		var feature = FeaturesHelper<TFeature>.Parse(unparsedFeature);
		this.Logger.TryingToDisableFeature(feature, context.Member!.DisplayName);
		try
		{
			await this.UserFeaturesService.DisableFeaturesAsync(feature, context.User.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is UserFeaturesException ufe
				? EmbedTemplate.ErrorEmbed(ufe.Message, $"Failed disabling {feature.Humanize()}").Build()
				: EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			this.Logger.FailedToDisableFeature(ex, feature, context.Member.DisplayName);
			throw;
		}

		this.Logger.SuccessfullyDisabledFeature(feature, context.Member.DisplayName);
		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully disabled {feature.Humanize()} for you"));
	}

	public virtual Task ListFeaturesCommand(InteractionContext context) => context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("All features")
		.WithDescription(string.Join(";\n", FeaturesHelper<TFeature>.Features.Select(x => $"[{x.Description}] - {x.Summary}"))));

	public virtual Task EnabledFeaturesCommand(InteractionContext context)
	{
		var featuresDesc = this.UserFeaturesService.EnabledFeatures(context.User.Id);
		return context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("Your enabled features").WithDescription(featuresDesc));
	}
}