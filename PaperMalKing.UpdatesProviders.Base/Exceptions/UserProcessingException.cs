using System;

namespace PaperMalKing.UpdatesProviders.Base.Exceptions
{
	public sealed class UserProcessingException : Exception
	{
		public BaseUser? User { get; }

		public UserProcessingException(BaseUser user, string message) : base(message) => this.User = user;

		public UserProcessingException(string message) : base(message)
		{ }
	}
}