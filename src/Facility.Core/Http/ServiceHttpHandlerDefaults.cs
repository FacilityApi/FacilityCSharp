namespace Facility.Core.Http;

/// <summary>
/// Settings for service HTTP handlers.
/// </summary>
public sealed class ServiceHttpHandlerDefaults
{
	/// <summary>
	/// The default JSON serializer.
	/// </summary>
	public JsonServiceSerializer? JsonSerializer { get; set; }
}
