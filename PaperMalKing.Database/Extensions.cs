using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PaperMalKing.Database
{
	public static class Extensions
	{
		public static async Task<int> SaveChangesAndThrowOnNoneAsync(this DbContext context, CancellationToken cancellationToken = default)
		{
			var rows = await context.SaveChangesAsync(cancellationToken);
			if (rows <= 0)
				throw new NoChangesSavedException(context);
			return rows;
		}
	}
}