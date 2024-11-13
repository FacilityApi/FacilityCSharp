// <auto-generated>
// DO NOT EDIT: generated by fsdgencsharp
// </auto-generated>
#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;

namespace EdgeCases
{
	/// <summary>
	/// A delegating implementation of EdgeCases.
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("fsdgencsharp", "")]
	public partial class DelegatingEdgeCases : IEdgeCases
	{
		/// <summary>
		/// Creates an instance with the specified service delegate.
		/// </summary>
		public DelegatingEdgeCases(ServiceDelegate serviceDelegate) =>
			m_serviceDelegate = serviceDelegate ?? throw new ArgumentNullException(nameof(serviceDelegate));

		/// <summary>
		/// Creates an instance with the specified delegator.
		/// </summary>
		[Obsolete("Use the constructor that accepts a ServiceDelegate.")]
		public DelegatingEdgeCases(ServiceDelegator delegator) =>
			m_serviceDelegate = ServiceDelegate.FromDelegator(delegator);

		/// <summary>
		/// An old method.
		/// </summary>
		[Obsolete]
		public virtual async Task<ServiceResult<OldMethodResponseDto>> OldMethodAsync(OldMethodRequestDto request, CancellationToken cancellationToken = default) =>
			(await m_serviceDelegate.InvokeMethodAsync(EdgeCasesMethods.OldMethod, request, cancellationToken).ConfigureAwait(false)).Cast<OldMethodResponseDto>();

		/// <summary>
		/// Custom HTTP method.
		/// </summary>
		public virtual async Task<ServiceResult<CustomHttpResponseDto>> CustomHttpAsync(CustomHttpRequestDto request, CancellationToken cancellationToken = default) =>
			(await m_serviceDelegate.InvokeMethodAsync(EdgeCasesMethods.CustomHttp, request, cancellationToken).ConfigureAwait(false)).Cast<CustomHttpResponseDto>();

		public virtual async Task<ServiceResult<SnakeMethodResponseDto>> SnakeMethodAsync(SnakeMethodRequestDto request, CancellationToken cancellationToken = default) =>
			(await m_serviceDelegate.InvokeMethodAsync(EdgeCasesMethods.SnakeMethod, request, cancellationToken).ConfigureAwait(false)).Cast<SnakeMethodResponseDto>();

		public virtual async Task<ServiceResult<MiscResponseDto>> MiscAsync(MiscRequestDto request, CancellationToken cancellationToken = default) =>
			(await m_serviceDelegate.InvokeMethodAsync(EdgeCasesMethods.Misc, request, cancellationToken).ConfigureAwait(false)).Cast<MiscResponseDto>();

		private readonly ServiceDelegate m_serviceDelegate;
	}
}
