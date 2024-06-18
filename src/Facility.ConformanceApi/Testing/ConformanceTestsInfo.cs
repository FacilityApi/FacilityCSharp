using System.Collections.Generic;
#if !NET8_0_OR_GREATER
using Facility.Core;
#endif

namespace Facility.ConformanceApi.Testing;

/// <summary>
/// Information for conformance tests.
/// </summary>
public sealed class ConformanceTestsInfo
{
	/// <summary>
	/// Load tests from JSON.
	/// </summary>
#if NET8_0_OR_GREATER
	public static ConformanceTestsInfo FromJson(string json) => ConformanceTestsJsonServiceSerializer.Instance.FromJson<ConformanceTestsInfo>(json)!;
#else
	public static ConformanceTestsInfo FromJson(string json) => SystemTextJsonServiceSerializer.Instance.FromJson<ConformanceTestsInfo>(json)!;
#endif

	/// <summary>
	/// The name of the test.
	/// </summary>
	public IReadOnlyList<ConformanceTestInfo>? Tests { get; set; }
}
