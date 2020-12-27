using System;

namespace PaperMalKing.UpdatesProviders.Base.Features
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class FeatureReadableNameAttribute : Attribute
	{
		public readonly string Name;

		public FeatureReadableNameAttribute(string name) => this.Name = name.ToLowerInvariant();
	}
}