namespace Facility.Core.Http;

/// <summary>
/// Defaults for HTTP client services.
/// </summary>
public sealed class HttpClientServiceDefaults
{
	/// <summary>
	/// The default base URI of the service.
	/// </summary>
	public Uri? BaseUri { get; set; }

	/// <summary>
	/// The default JSON serializer.
	/// </summary>
	public JsonServiceSerializer? JsonSerializer { get; set; }
}
