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

using System;
using System.Collections.Generic;
using System.Reflection;

namespace PaperMalKing.UpdatesProviders.Base.Features
{
	public static class BaseFeaturesHelper
	{
		static BaseFeaturesHelper()
		{
			Features = new Dictionary<Type, IReadOnlyList<string>>();
		}

		private static readonly Dictionary<Type, IReadOnlyList<string>> Features;

		public static IReadOnlyList<string> ListFeatures<TFeature>() where TFeature : struct, Enum
		{
			var featureType = typeof(TFeature);
			if (Features.ContainsKey(featureType))
			{
				return Features[featureType];
			}

			var enumValues = featureType.GetFields();
			var res = new string[enumValues.Length];
			for (var i = 0; i < enumValues.Length; i++)
			{
				var enumValue = enumValues[i];
				var frnAttr = enumValue.GetCustomAttribute<FeatureReadableNameAttribute>();
				if (frnAttr != null)
					res[i] = frnAttr.Name;
				else
					res[i] = enumValue.Name;
			}

			Features.Add(featureType, res);
			return res;
		}

		public static bool TryParse<TFeature>(string value, out TFeature result) where TFeature : struct, Enum
		{
			value = value.ToLowerInvariant();
			var featureType = typeof(TFeature);
			var enumValues = featureType.GetFields();
			foreach (var enumValue in enumValues)
			{
				var parseResult = enumValue.GetValue(null);
				if (parseResult == null)
					continue;

				if (enumValue.Name == value)
				{
					result = (TFeature) parseResult;
					return true;
				}

				var frnAtt = enumValue.GetCustomAttribute<FeatureReadableNameAttribute>();
				if (frnAtt?.Name == value)
				{
					result = (TFeature) parseResult;
					return true;
				}
			}

			result = default;
			return false;
		}
	}
}