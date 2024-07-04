using System.Diagnostics.CodeAnalysis;

namespace Facility.Core;

/// <summary>
/// Used to delegate a service.
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Best alternative to obsolete ServiceDelegator.")]
public abstract class ServiceDelegate
{
	/// <summary>
	/// Delegates the service method.
	/// </summary>
	public virtual Task<ServiceResult<ServiceDto>> InvokeMethodAsync(IServiceMethodInfo methodInfo, ServiceDto request, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	/// <summary>
	/// Delegates the service event.
	/// </summary>
	public virtual Task<ServiceResult<IAsyncEnumerable<ServiceResult<ServiceDto>>>> InvokeEventAsync(IServiceEventInfo eventInfo, ServiceDto request, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	/// <summary>
	/// Creates a service delegate from a service delegator.
	/// </summary>
	public static ServiceDelegate FromDelegator(ServiceDelegator delegator) => new DelegatorServiceDelegate(delegator);

	private sealed class DelegatorServiceDelegate : ServiceDelegate
	{
		public DelegatorServiceDelegate(ServiceDelegator delegator) => m_delegator = delegator ?? throw new ArgumentNullException(nameof(delegator));

		public override Task<ServiceResult<ServiceDto>> InvokeMethodAsync(IServiceMethodInfo methodInfo, ServiceDto request, CancellationToken cancellationToken = default) =>
			m_delegator(methodInfo, request, cancellationToken);

		private readonly ServiceDelegator m_delegator;
	}
}
