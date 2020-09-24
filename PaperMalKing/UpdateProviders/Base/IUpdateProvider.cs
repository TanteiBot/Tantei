using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using PaperMalKing.Services;

namespace PaperMalKing.UpdateProviders.Base
{
	public interface IUpdateProvider
	{
		string Name { get; }
		
		Task GetUpdates(CancellationToken cancellationToken = default);

		event UpdateFoundEvent UpdateFound;
	}
}