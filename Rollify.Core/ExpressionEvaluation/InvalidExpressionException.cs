using System;

namespace Rollify.Core
{
	public class InvalidExpressionException : Exception
	{

		public static InvalidExpressionException DEFAULT = new InvalidExpressionException("Invalid Expression");

		public InvalidExpressionException (string message)
			: base(message)
		{
			
		}

		public bool Equals(InvalidExpressionException other) {
			return this.Message == other.Message;
		}
	}
}

