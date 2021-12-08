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
	[Obsolete("Use the overload with the serializer.")]
	public static ConformanceTestsInfo FromJson(string json) => FromJson(json, NewtonsoftJsonServiceSerializer.Instance);

	/// <summary>
	/// Load tests from JSON.
	/// </summary>
	public static ConformanceTestsInfo FromJson(string json, JsonServiceSerializer jsonSerializer) => jsonSerializer.FromJson<ConformanceTestsInfo>(json)!;

	/// <summary>
	/// The name of the test.
	/// </summary>
	public IReadOnlyList<ConformanceTestInfo>? Tests { get; set; }
}
