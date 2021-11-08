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

namespace Tantei.Core.Models.Medias;

public sealed record Media(string Name, Uri Url, IReadOnlyList<SubEntry> Entries, Format Format, Status Status, Uri? ImageUrl = null,
						   string? Notes = null, IReadOnlyList<Tag>? Tags = null, IReadOnlyList<string>? Genres = null, string? Description = null,
						   IReadOnlyList<Studio>? Studios = null, IReadOnlyCollection<Staff>? Staff = null) : INameUrlable, IHasImage;