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

using System.Text;
using PaperMalKing.AniList.Wrapper.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class Helpers
{
	private const string Media = @"
                      title {
                        stylisedRomaji: romaji(stylised: true)
                        romaji(stylised: false)
                        stylisedEnglish: english(stylised: true)
                        english(stylised: false)
                        stylisedNative: native(stylised: true)
                        native(stylised: false)
                      }
                      countryOfOrigin
                      type
                      siteUrl
                      id
                      image: coverImage {
                        large: extraLarge
                      }";

	internal static StringBuilder AppendMediaFields(StringBuilder sb, RequestOptions options)
	{
		sb.AppendLine(Media);
		if ((options & RequestOptions.MediaDescription) != 0)
			sb.AppendLine("description(asHtml: false)");

		if ((options & RequestOptions.Genres) != 0)
			sb.AppendLine("genres");
		if ((options & RequestOptions.Tags) != 0)
			sb.AppendLine(@"tags{
                      name
                      rank
                      isMediaSpoiler
                    }");

		if ((options & RequestOptions.MediaFormat) != 0)
			sb.AppendLine(@"format
        countryOfOrigin");
		if ((options & RequestOptions.MediaStatus) != 0)
			sb.AppendLine("status(version: 2)");
		if ((options & RequestOptions.Genres) != 0)
			sb.AppendLine("genres");
		if ((options & RequestOptions.Tags) != 0)
			sb.AppendLine(@"tags{
                      name
                      rank
                      isMediaSpoiler
                    }");
		if ((options & RequestOptions.Studio) != 0)
			sb.AppendLine(@"studios(sort: FAVOURITES_DESC, isMain: true){
                      values: nodes{
                        name
                        siteUrl
						isAnimationStudio
                      }
                    }");
		if ((options & RequestOptions.Mangaka) != 0)
			sb.AppendLine(@"staff(sort: FAVOURITES_DESC, page: 1, perPage: 5){
                      values: edges{
                        role
                        node{
                          name{
                            full
                            native
                          }
                          siteUrl
                        }
                      }
                    }");
		return sb;
	}
}