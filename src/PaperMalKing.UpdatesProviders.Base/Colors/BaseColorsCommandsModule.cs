// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Colors;

public abstract class BaseColorsCommandsModule<TUser, TUpdateType>
	(ILogger<BaseColorsCommandsModule<TUser, TUpdateType>> logger, CustomColorService<TUser, TUpdateType> colorService) : BotCommandsModule
	where TUser : class, IUpdateProviderUser
	where TUpdateType : unmanaged, Enum
{
	protected override bool IsResponseVisibleOnlyForRequester => true;

	public virtual async Task SetColor(InteractionContext context, string unparsedUpdateType, string colorValue)
	{
		using var scope = CreateLoggerScope(logger, context);
		try
		{
			var color = new DiscordColor(colorValue);
			var updateType = UpdateTypesHelper<TUpdateType>.Parse(unparsedUpdateType);
			await colorService.SetColorAsync(context.User.Id, updateType, color);
			await context.EditResponseAsync(EmbedTemplate.SuccessEmbed($"Successfully set color of {updateType}"));
		}
		catch (Exception ex)
		{
			var embed = ex is ArgumentException or UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			logger.FailedToSetColor(ex, unparsedUpdateType, colorValue);
			throw;
		}
	}

	public virtual async Task RemoveColor(InteractionContext context, string unparsedUpdateType)
	{
		using var scope = CreateLoggerScope(logger, context);
		try
		{
			var updateType = UpdateTypesHelper<TUpdateType>.Parse(unparsedUpdateType);
			await colorService.RemoveColorAsync(context.User.Id, updateType);
			await context.EditResponseAsync(EmbedTemplate.SuccessEmbed($"Successfully removed color of {updateType}"));
		}
		catch (Exception ex)
		{
			var embed = ex is ArgumentException or UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			logger.FailedToRemoveColor(ex, unparsedUpdateType);
			throw;
		}
	}

	public virtual async Task ListOverridenColor(InteractionContext context)
	{
		using var scope = CreateLoggerScope(logger, context);
		var colors = colorService.OverridenColors(context.User.Id);
		await context.EditResponseAsync(EmbedTemplate
										 .SuccessEmbed(string.IsNullOrWhiteSpace(colors) ? "You have no colors overriden" : "Your overriden colors")
										 .WithDescription(colors));
	}
}