using Facility.Core;

namespace Facility.ConformanceApi.Testing;

/// <summary>
/// Settings for <see cref="ConformanceApiTester"/>.
/// </summary>
public sealed class ConformanceApiTesterSettings
{
	/// <summary>
	/// The tests.
	/// </summary>
	public IReadOnlyList<ConformanceTestInfo>? Tests { get; set; }

	/// <summary>
	/// The API to test.
	/// </summary>
	public IConformanceApi? Api { get; set; }

	/// <summary>
	/// The JSON serializer to use.
	/// </summary>
	public JsonServiceSerializer? JsonSerializer { get; set; }

	/// <summary>
	/// The HTTP client to use (optional).
	/// </summary>
	public HttpClient? HttpClient { get; set; }
}
