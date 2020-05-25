using System.Collections.Concurrent;

namespace PaperMalKing.Data
{
	/// <summary>
	/// Queue with fixed size
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class FixedSizeQueue<T> : ConcurrentQueue<T>
	{
		/// <summary>
		/// Object for locking
		/// </summary>
		private readonly object _lockObject;

		/// <summary>
		/// Limit for amount of elements in queue
		/// </summary>
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