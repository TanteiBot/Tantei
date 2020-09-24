using System.Threading.Tasks;
using PaperMalKing.UpdateProviders.Base.EventArgs;

namespace PaperMalKing.UpdateProviders.Base
{
	public delegate Task UpdateFoundEvent(UpdateFoundEventArgs args);
}