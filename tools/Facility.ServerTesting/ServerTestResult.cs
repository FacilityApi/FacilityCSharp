namespace Facility.ServerTesting
{
	public sealed class ServerTestResult
	{
		public ServerTestResult(string test, ServerTestStatus status, string message = null)
		{
			Test = test;
			Status = status;
			Message = message;
		}

		public string Test { get; }

		public ServerTestStatus Status { get; }

		public string Message { get; }
	}
}
