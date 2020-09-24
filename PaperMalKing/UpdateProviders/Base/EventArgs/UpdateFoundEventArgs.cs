using System.Collections.Generic;

namespace PaperMalKing.UpdateProviders.Base.EventArgs
{
	public class UpdateFoundEventArgs
	{
		public IReadOnlyList<IViewableUpdate> UpdatesFound { get; }
	}
}