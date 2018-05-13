using System;

namespace Facility.ServerTesting
{
	public sealed class TestFailedException : Exception
	{
		public TestFailedException(string message)
			: base(message)
		{
		}
	}
}
