using Facility.Core;

namespace Facility.ConformanceApi.Testing;

/// <summary>
/// Information for conformance tests.
/// </summary>
public sealed class ConformanceTestsInfo
{
	/// <summary>
	/// Load tests from JSON.
	/// </summary>
	[Obsolete("Use the overload with ServiceSerializer.")]
	public static ConformanceTestsInfo FromJson(string json) => FromJson(json, ServiceSerializer.Default);

	/// <summary>
	/// Load tests from JSON.
	/// </summary>
	public static ConformanceTestsInfo FromJson(string json, ServiceSerializer serializer) => serializer.FromString<ConformanceTestsInfo>(json)!;

	/// <summary>
	/// The name of the test.
	/// </summary>
	public IReadOnlyList<ConformanceTestInfo>? Tests { get; set; }
}
