using System.Collections.Generic;

namespace Facility.ConformanceApi.Testing
{
	/// <summary>
	/// Provides conformance tests.
	/// </summary>
	public interface IConformanceTestProvider
	{
		/// <summary>
		/// The test names.
		/// </summary>
		IReadOnlyList<string> GetTestNames();

		/// <summary>
		/// Gets information for the test with the specified name.
		/// </summary>
		ConformanceTestInfo TryGetTestInfo(string testName);
	}
}
