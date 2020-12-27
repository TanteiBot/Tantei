using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PaperMalKing.UpdatesProviders.Base.UpdateProvider
{
	/// <summary>
	/// Must contain public static method named Configure accepting <see cref="IConfiguration"/> and <see cref="IServiceCollection"/>
	/// </summary>
	public interface IUpdateProviderConfigurator<T> where T : IUpdateProvider
	{
		void ConfigureNonStatic(IConfiguration configuration, IServiceCollection serviceCollection);
	}
}