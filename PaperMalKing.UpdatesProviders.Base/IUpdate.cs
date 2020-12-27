using System.Collections.Generic;
using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base
{
	public interface IUpdate
	{
		IReadOnlyList<DiscordEmbedBuilder> UpdateEmbeds { get; }
	}
}