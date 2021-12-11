namespace Facility.Core.Http;

/// <summary>
/// Defaults for service HTTP handlers.
/// </summary>
public sealed class ServiceHttpHandlerDefaults
{
	/// <summary>
	/// The default content serializer.
	/// </summary>
	public HttpContentSerializer? ContentSerializer { get; set; }
}
