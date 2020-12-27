using System;

namespace PaperMalKing.UpdatesProviders.Base.Exceptions
{
	public sealed class UserProcessingException : Exception
	{
		public IUser? User { get; }

		public UserProcessingException(IUser user, string message) : base(message) => this.User = user;

		public UserProcessingException(string message) : base(message)
		{ }
	}
}