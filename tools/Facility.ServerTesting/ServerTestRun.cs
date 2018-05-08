using System.Collections.Generic;

namespace Facility.ServerTesting
{
	public sealed class ServerTestRun
	{
		public ServerTestRun(IReadOnlyList<ServerTestResult> results)
		{
			Results = results;
		}

		public IReadOnlyList<ServerTestResult> Results { get; }
	}
}
