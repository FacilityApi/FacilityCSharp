using System.Runtime.CompilerServices;

namespace Facility.Core;

/// <summary>
/// Common service delegates.
/// </summary>
public static class ServiceDelegates
{
	/// <summary>
	/// All methods throw <see cref="NotImplementedException"/>.
	/// </summary>
	public static ServiceDelegate NotImplemented => NotImplementedServiceDelegate.Instance;

	/// <summary>
	/// Forwards all methods to the inner service.
	/// </summary>
	public static ServiceDelegate Forward(object inner) => new ForwardingServiceDelegate(inner);

	/// <summary>
	/// Validates requests and responses.
	/// </summary>
	public static ServiceDelegate Validate(object inner) => new ValidatingServiceDelegate(inner);

	private sealed class NotImplementedServiceDelegate : ServiceDelegate
	{
		public static ServiceDelegate Instance { get; } = new NotImplementedServiceDelegate();
	}

	private sealed class ForwardingServiceDelegate : ServiceDelegate
	{
		public ForwardingServiceDelegate(object inner) => m_inner = inner ?? throw new ArgumentNullException(nameof(inner));

		public override async Task<ServiceResult<ServiceDto>> InvokeMethodAsync(IServiceMethodInfo methodInfo, ServiceDto request, CancellationToken cancellationToken = default) =>
			await methodInfo.InvokeAsync(m_inner, request, cancellationToken).ConfigureAwait(false);

		public override async Task<ServiceResult<IAsyncEnumerable<ServiceResult<ServiceDto>>>> InvokeEventAsync(IServiceEventInfo eventInfo, ServiceDto request, CancellationToken cancellationToken = default) =>
			await eventInfo.InvokeAsync(m_inner, request, cancellationToken).ConfigureAwait(false);

		private readonly object m_inner;
	}

	private sealed class ValidatingServiceDelegate : ServiceDelegate
	{
		public ValidatingServiceDelegate(object inner) => m_inner = inner ?? throw new ArgumentNullException(nameof(inner));

		public override async Task<ServiceResult<ServiceDto>> InvokeMethodAsync(IServiceMethodInfo methodInfo, ServiceDto request, CancellationToken cancellationToken = default)
		{
			if (request is null)
				throw new ArgumentNullException(nameof(request));

			if (!request.Validate(out var requestErrorMessage))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest(requestErrorMessage));

			var response = await methodInfo.InvokeAsync(m_inner, request, cancellationToken).ConfigureAwait(false);

			if (!response.Validate(out var responseErrorMessage))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidResponse(responseErrorMessage));

			return response;
		}

		public override async Task<ServiceResult<IAsyncEnumerable<ServiceResult<ServiceDto>>>> InvokeEventAsync(IServiceEventInfo eventInfo, ServiceDto request, CancellationToken cancellationToken = default)
		{
			if (request is null)
				throw new ArgumentNullException(nameof(request));

			if (!request.Validate(out var requestErrorMessage))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest(requestErrorMessage));

			var result = await eventInfo.InvokeAsync(m_inner, request, cancellationToken).ConfigureAwait(false);
			if (result.IsFailure)
				return result.ToFailure();

			return ServiceResult.Success(Enumerate(result.Value, cancellationToken));

			static async IAsyncEnumerable<ServiceResult<ServiceDto>> Enumerate(IAsyncEnumerable<ServiceResult<ServiceDto>> enumerable, [EnumeratorCancellation] CancellationToken cancellationToken)
			{
				await foreach (var result in enumerable.WithCancellation(cancellationToken))
				{
					if (!result.Validate(out var resultErrorMessage))
					{
						yield return ServiceResult.Failure(ServiceErrors.CreateInvalidResponse(resultErrorMessage));
						yield break;
					}

					yield return result;
				}
			}
		}

		private readonly object m_inner;
	}
}
