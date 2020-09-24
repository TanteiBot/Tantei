using DSharpPlus.Entities;

namespace PaperMalKing.UpdateProviders.Base
{
	public interface IViewableUpdate
	{
		DiscordEmbed BuildView();
	}
}