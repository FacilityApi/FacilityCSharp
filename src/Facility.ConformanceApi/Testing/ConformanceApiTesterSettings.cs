using Facility.Core;

namespace Facility.ConformanceApi.Testing;

/// <summary>
/// Tests a conformance API service.
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
	/// The service serializer to use.
	/// </summary>
	public ServiceSerializer? ServiceSerializer { get; set; }

	/// <summary>
	/// The HTTP client to use (optional).
	/// </summary>
	public HttpClient? HttpClient { get; set; }
}
