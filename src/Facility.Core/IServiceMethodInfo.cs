using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core
{
	/// <summary>
	/// Information about a Facility service method.
	/// </summary>
	/// <remarks>Do not implement this interface. New members on this interface
	/// will not be considered a breaking change.</remarks>
	public interface IServiceMethodInfo
	{
		/// <summary>
		/// The name of the method.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The name of the service.
		/// </summary>
		string ServiceName { get; }

		/// <summary>
		/// Invokes the method on the specified service instance.
		/// </summary>
		Task<ServiceResult<ServiceDto>> InvokeAsync(object service, ServiceDto request, CancellationToken cancellationToken);
	}
}
