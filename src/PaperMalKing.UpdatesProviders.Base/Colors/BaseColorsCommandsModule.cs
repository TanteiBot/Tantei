// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Colors;

public abstract class BaseColorsCommandsModule<TUser, TUpdateType> : BotCommandsModule
	where TUser : class, IUpdateProviderUser
	where TUpdateType : unmanaged, Enum
{
	protected ILogger<BaseColorsCommandsModule<TUser, TUpdateType>> Logger { get; }

	protected CustomColorService<TUser, TUpdateType> ColorService { get; }

	protected override bool IsResponseVisibleOnlyForRequester => true;

	protected BaseColorsCommandsModule(ILogger<BaseColorsCommandsModule<TUser, TUpdateType>> logger, CustomColorService<TUser, TUpdateType> colorService)
	{
		this.Logger = logger;
		this.ColorService = colorService;
	}

	public virtual async Task SetColor(InteractionContext context, string unparsedUpdateType, string colorValue)
	{
		TUpdateType updateType;
		try
		{
			var color = new DiscordColor(colorValue);
			updateType = UpdateTypesHelper<TUpdateType>.Parse(unparsedUpdateType);
			await this.ColorService.SetColorAsync(context.User.Id, updateType, color);
		}
		catch (Exception ex)
		{
			var embed = ex is ArgumentException or UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			this.Logger.FailedToSetColor(ex, unparsedUpdateType, colorValue);
			throw;
		}

		await context.EditResponseAsync(EmbedTemplate.SuccessEmbed($"Successfully set color of {updateType}"));
	}

	public virtual async Task RemoveColor(InteractionContext context, string unparsedUpdateType)
	{
		TUpdateType updateType;
		try
		{
			updateType = UpdateTypesHelper<TUpdateType>.Parse(unparsedUpdateType);
			await this.ColorService.RemoveColorAsync(context.User.Id, updateType);
		}
		catch (Exception ex)
		{
			var embed = ex is ArgumentException or UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			this.Logger.FailedToRemoveColor(ex, unparsedUpdateType);
			throw;
		}

		await context.EditResponseAsync(EmbedTemplate.SuccessEmbed($"Successfully removed color of {updateType}"));
	}

	public virtual Task<DiscordMessage> ListOverridenColor(InteractionContext context)
	{
		var colors = this.ColorService.OverridenColors(context.User.Id);
		return context.EditResponseAsync(EmbedTemplate
										 .SuccessEmbed(string.IsNullOrWhiteSpace(colors) ? "You have no colors overriden" : "Your overriden colors")
										 .WithDescription(colors));
	}
}