using System.Collections.Generic;

namespace PaperMalKing.Data
{
	public class FixedSizeQueue<T> : Queue<T>
	{
		public int Limit { get; set; }

		public FixedSizeQueue(int limit)
		{
			this.Limit = limit;
		}

		public new void Enqueue(T item)
		{
			while (this.Limit >= this.Count)
				this.Dequeue();

			base.Enqueue(item);
		}
	}
}