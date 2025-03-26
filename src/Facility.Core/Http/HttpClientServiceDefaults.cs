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
	/// The default content serializer.
	/// </summary>
	public HttpContentSerializer? ContentSerializer { get; set; }

	/// <summary>
	/// True to compress all requests by default.
	/// </summary>
	public bool? CompressRequests { get; set; }
}
