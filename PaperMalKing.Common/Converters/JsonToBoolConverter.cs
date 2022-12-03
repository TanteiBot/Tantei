#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.Common.Converters
{
	public sealed class JsonToBoolConverter : JsonConverter<bool>
	{
		/// <inheritdoc />
		public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
		{
			JsonTokenType.Number => reader.GetByte() == 1,
			JsonTokenType.String => reader.GetString() == "1",
			JsonTokenType.True   => true,
			_                    => false
		};

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
			writer.WriteBoolean("is_reprogressing", value);
	}
}