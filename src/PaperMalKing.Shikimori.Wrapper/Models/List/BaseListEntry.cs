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

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.List;

internal abstract class BaseListEntry<T> where T : BaseListSubEntry
{
	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("score")]
	public byte Score { get; init; }

	[JsonPropertyName("status")]
	public AnimeStatus Status { get; init; }

	[JsonPropertyName("text")]
	public string? Text { get; init; }

	[JsonPropertyName("rewatches")]
	public int Rewatches { get; init; }

	[JsonPropertyName("updated_at")]
	public DateTimeOffset UpdatedAt { get; init; }

	internal abstract T SubEntry { get; init; }
}