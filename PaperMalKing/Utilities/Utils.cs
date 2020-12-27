using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PaperMalKing.Utilities
{
	public static class Utils
	{
		public static IReadOnlyList<Assembly> LoadAndListPmkAssemblies()
		{
			var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "PaperMalKing.*.dll");
			foreach (var file in files)
			{
				Assembly.LoadFile(file);
			}
			return AppDomain.CurrentDomain.GetAssemblies();
		}
	}
}