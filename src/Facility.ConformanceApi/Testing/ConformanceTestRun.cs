namespace Facility.ConformanceApi.Testing
{
	/// <summary>
	/// The results of a test run.
	/// </summary>
	public sealed class ConformanceTestRun
	{
		/// <summary>
		/// Creates a test run from results.
		/// </summary>
		public ConformanceTestRun(IReadOnlyList<ConformanceTestResult> results)
		{
			Results = results;
		}

		/// <summary>
		/// The test results.
		/// </summary>
		public IReadOnlyList<ConformanceTestResult> Results { get; }
	}
}
