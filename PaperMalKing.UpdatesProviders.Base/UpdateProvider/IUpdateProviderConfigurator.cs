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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PaperMalKing.UpdatesProviders.Base.UpdateProvider
{
	/// <summary>
	/// Must contain public static method named Configure accepting <see cref="IConfiguration"/> and <see cref="IServiceCollection"/>
	/// </summary>
	public interface IUpdateProviderConfigurator<T> where T : IUpdateProvider
	{
		void ConfigureNonStatic(IConfiguration configuration, IServiceCollection serviceCollection);
	}
}