namespace Facility.Core;

/// <summary>
/// Information about a Facility service event.
/// </summary>
/// <remarks>Do not implement this interface. New members on this interface
/// will not be considered a breaking change.</remarks>
public interface IServiceEventInfo
{
	/// <summary>
	/// The name of the event.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// The name of the service.
	/// </summary>
	string ServiceName { get; }

	/// <summary>
	/// Invokes the method on the specified service instance.
	/// </summary>
	Task<ServiceResult<IAsyncEnumerable<ServiceResult<ServiceDto>>>> InvokeAsync(object service, ServiceDto request, CancellationToken cancellationToken = default);
}
