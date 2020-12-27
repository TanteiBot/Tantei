using System.Threading.Tasks;

namespace PaperMalKing.UpdatesProviders.Base.UpdateProvider
{
	public interface IUpdateProvider
	{
		string Name { get; }
		event UpdateFoundEventHandler UpdateFoundEvent;

		public Task TriggerStoppingAsync();
	}
}