using System.Collections.Generic;

namespace Facility.ConformanceApi.Testing
{
	/// <summary>
	/// Information for conformance tests.
	/// </summary>
	public sealed class ConformanceTestsInfo
	{
		/// <summary>
		/// The name of the test.
		/// </summary>
		public IReadOnlyList<ConformanceTestInfo> Tests { get; set; }
	}
}
