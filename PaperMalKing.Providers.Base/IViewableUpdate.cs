using DSharpPlus.Entities;

namespace PaperMalKing.Providers.Base
{
	public interface IViewableUpdate
	{
		DiscordEmbed BuildView();
	}
}