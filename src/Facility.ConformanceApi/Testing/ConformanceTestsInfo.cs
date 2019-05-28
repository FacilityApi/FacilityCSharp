using System.Collections.Generic;
using Facility.Core;

namespace Facility.ConformanceApi.Testing
{
	/// <summary>
	/// Information for conformance tests.
	/// </summary>
	public sealed class ConformanceTestsInfo
	{
		/// <summary>
		/// Load tests from JSON.
		/// </summary>
		public static ConformanceTestsInfo FromJson(string json) => ServiceJsonUtility.FromJson<ConformanceTestsInfo>(json);

		/// <summary>
		/// The name of the test.
		/// </summary>
		public IReadOnlyList<ConformanceTestInfo> Tests { get; set; }
	}
}
