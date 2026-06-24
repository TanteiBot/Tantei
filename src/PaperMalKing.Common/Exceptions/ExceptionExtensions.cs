// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.Common.Exceptions;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "Compiler error https://github.com/dotnet/sdk/issues/51716")]
[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "There is no this")]
public static class ExceptionExtensions
{
	extension(ArgumentException)
	{
		[DoesNotReturn]
		public static T Throw<T>(string message, string paramName)
		{
			throw new ArgumentException(message, paramName);
		}
	}

	extension(ArgumentOutOfRangeException)
	{
		[DoesNotReturn]
		public static T Throw<T>(string paramName, object? value, string? message)
		{
			throw new ArgumentOutOfRangeException(paramName, value, message);
		}
	}

	extension(InvalidOperationException)
	{
		[DoesNotReturn]
		public static T Throw<T>()
		{
			throw new InvalidOperationException();
		}
	}

	extension(Exception ex)
	{
		public string FullMessage
		{
			get
			{
				if (ex.InnerException is null)
				{
					return ex.Message;
				}

				return GetMessage(ex).JoinToString(";\n");

				static IEnumerable<string> GetMessage(Exception exception)
				{
					while (true)
					{
						if (!string.IsNullOrWhiteSpace(exception.Message))
						{
							yield return exception.Message;
						}

						if (exception.InnerException is not null)
						{
							exception = exception.InnerException;
							continue;
						}

						break;
					}
				}
			}
		}
	}
}