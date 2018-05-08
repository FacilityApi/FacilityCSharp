using System;

namespace Facility.ServerTesting
{
	internal sealed class TestFailedException : Exception
	{
		public TestFailedException(string message)
			: base(message)
		{
		}
	}
}
