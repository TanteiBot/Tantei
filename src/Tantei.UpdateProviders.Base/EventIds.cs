// Tantei.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace Tantei.UpdateProviders.Base;

internal static class EventIds
{
	public const int UpdateCheckStart = 0;

	public const int UpdateCheckError = 2;

	public const int UpdateCheckFinish = 3;

	public const int TryingToStartAlreadyStartedUpdateProvider = 4;

	public const int StartingUpdateProvider = 5;
}