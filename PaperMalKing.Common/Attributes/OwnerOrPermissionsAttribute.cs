#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Diagnostics.CodeAnalysis;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace PaperMalKing.Common.Attributes;

/// <summary>
/// Defines that command or group of commands can only be executed by owner of the bot or user with specified permissions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class OwnerOrPermissionAttribute : CheckBaseAttribute
{
	/// <summary>
	/// Permissions needed to execute command.
	/// </summary>
	public Permissions Permissions { get; }

	/// <summary>
	/// Defines that command or group of commands can only be executed by owner of the bot or user with specified permissions.
	/// </summary>
	/// <param name="permissions">Permissions needed to execute command.</param>
	public OwnerOrPermissionAttribute(Permissions permissions) => this.Permissions = permissions;

	public override Task<bool> ExecuteCheckAsync([NotNull] CommandContext ctx, bool help)
	{
		var app = ctx.Client.CurrentApplication;
		var me = ctx.Client.CurrentUser;

		if (app != null && app.Owners.Any(x => x.Id == ctx.User.Id))
			return Task.FromResult(true);

		if (ctx.User.Id == me.Id)
			return Task.FromResult(true);

		var usr = ctx.Member;
		if (usr == null)
			return Task.FromResult(false);
		var pusr = ctx.Channel.PermissionsFor(usr);

		return Task.FromResult((pusr & this.Permissions) == this.Permissions);
	}
}