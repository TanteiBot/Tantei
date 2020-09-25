using System;
using System.Threading;
using System.Threading.Tasks;
using PaperMalKing.Services;

namespace PaperMalKing.Database.Repositories
{
	public class BaseRepository : IDisposable
	{
		protected readonly DatabaseContext _context;

		public BaseRepository(DatabaseContext context)
		{
			this._context = context;
		}

		public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
			this._context.SaveChangesAsync(cancellationToken);

		/// <inheritdoc />
		public virtual void Dispose()
		{
			this._context.Dispose();
		}
	}
}