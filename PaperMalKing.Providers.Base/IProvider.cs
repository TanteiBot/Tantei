using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PaperMalKing.Providers.Base
{
	public interface IProvider
	{
		string Name { get; }

		IReadOnlyList<string> Aliases { get; }

		IUserManager UserManager { get; }

		ValueTask ConfigureServices(IServiceCollection services);

		Task GetUpdates(ChannelWriter<IViewableUpdate> channelWriter, CancellationToken cancellationToken = default);
	}
}