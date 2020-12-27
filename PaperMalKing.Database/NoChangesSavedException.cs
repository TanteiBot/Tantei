using System;
using Microsoft.EntityFrameworkCore;

namespace PaperMalKing.Database
{
	public sealed class NoChangesSavedException : Exception
	{
		public DbContext? Context { get; }

		public NoChangesSavedException(DbContext? context)
		{
			this.Context = context;
		}
	}
}