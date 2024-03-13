// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace PaperMalKing.Common.Attributes;

/// <summary>
/// Defines that command or group of commands can only be executed by owner of the bot or user with specified permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class OwnerOrPermissionsAttribute : SlashCheckBaseAttribute
{
	/// <summary>
	/// Gets permissions needed to execute command.
	/// </summary>
	public Permissions Permissions { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="OwnerOrPermissionsAttribute"/> class.
	/// Defines that command or group of commands can only be executed by owner of the bot or user with specified permissions.
	/// </summary>
	/// <param name="permissions">Permissions needed to execute command.</param>
	public OwnerOrPermissionsAttribute(Permissions permissions)
	{
		this.Permissions = permissions;
	}

	public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
	{
		if (ctx.User.Id == ctx.Client.CurrentUser.Id || ctx.Client.CurrentApplication.Owners.Any(x => x.Id == ctx.User.Id))
		{
			return Task.FromResult(true);
		}

		var usr = ctx.Member;
		if (usr is null)
		{
			return Task.FromResult(false);
		}

		var pusr = ctx.Channel.PermissionsFor(usr);

		return Task.FromResult((pusr & this.Permissions) == this.Permissions);
	}
}