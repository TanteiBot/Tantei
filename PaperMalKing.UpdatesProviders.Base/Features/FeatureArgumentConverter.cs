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
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Humanizer;

namespace PaperMalKing.UpdatesProviders.Base.Features
{
	public class FeatureArgumentConverter<T> : IArgumentConverter<T> where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
	{
		[SuppressMessage("Microsoft.Design", "CA1031")]
		public Task<Optional<T>> ConvertAsync(string value, CommandContext ctx)
		{
			try
			{
				return Task.FromResult(new Optional<T>(value.DehumanizeTo<T>()));
			}
			catch
			{
				return Task.FromResult(Optional.FromNoValue<T>());
			}
		}
	}
}