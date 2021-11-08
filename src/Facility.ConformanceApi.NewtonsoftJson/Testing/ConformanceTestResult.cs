namespace Facility.ConformanceApi.Testing
{
	/// <summary>
	/// A conformance test result.
	/// </summary>
	public sealed class ConformanceTestResult
	{
		/// <summary>
		/// Creates a conformance test result.
		/// </summary>
		public ConformanceTestResult(string testName, ConformanceTestStatus status, string? message = null)
		{
			TestName = testName;
			Status = status;
			Message = message;
		}

		/// <summary>
		/// The name of the test.
		/// </summary>
		public string TestName { get; }

		/// <summary>
		/// The test status.
		/// </summary>
		public ConformanceTestStatus Status { get; }

		/// <summary>
		/// The test message, if any.
		/// </summary>
		public string? Message { get; }
	}
}
