using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PaperMalKing.Data
{
	public class FixedSizeQueue<T> : ConcurrentQueue<T>
	{
		private readonly object _lockObject;
		public int Limit { get; set; }

		public FixedSizeQueue(int limit)
		{
			this.Limit = limit;
			this._lockObject = new object();
		}

		public new void Enqueue(T item)
		{
			base.Enqueue(item);
			lock (this._lockObject)
			{
				while (this.Limit > base.Count)
				{
					base.TryDequeue(out _);
				}
			}
		}
	}
}