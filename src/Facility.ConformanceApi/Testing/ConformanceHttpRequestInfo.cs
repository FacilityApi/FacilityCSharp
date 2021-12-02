namespace Facility.ConformanceApi.Testing;

/// <summary>
/// A raw HTTP request.
/// </summary>
public sealed class ConformanceHttpRequestInfo
{
	/// <summary>
	/// The HTTP method being called.
	/// </summary>
	public string? Method { get; set; }

	/// <summary>
	/// The HTTP path.
	/// </summary>
	public string? Path { get; set; }
}
