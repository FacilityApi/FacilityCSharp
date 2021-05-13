using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core
{
	/// <summary>
	/// Called when delegating a service method.
	/// </summary>
	public delegate Task<ServiceResult<ServiceDto>> ServiceDelegator(IServiceMethodInfo method, ServiceDto request, CancellationToken cancellationToken);
}
