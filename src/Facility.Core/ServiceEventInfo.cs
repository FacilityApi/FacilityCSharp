using System.Runtime.CompilerServices;

namespace Facility.Core;

/// <summary>
/// Helpers for service event information.
/// </summary>
public static class ServiceEventInfo
{
	/// <summary>
	/// Creates service event information.
	/// </summary>
	/// <remarks>For internal use by generated code.</remarks>
	public static IServiceEventInfo Create<TApi, TRequestDto, TResponseDto>(string name, string serviceName, Func<TApi, Func<TRequestDto, CancellationToken, Task<ServiceResult<IAsyncEnumerable<ServiceResult<TResponseDto>>>>>> getInvokeAsync)
		where TRequestDto : ServiceDto
		where TResponseDto : ServiceDto
	{
		async Task<ServiceResult<IAsyncEnumerable<ServiceResult<ServiceDto>>>> InvokeAsync(object api, ServiceDto request, CancellationToken cancellationToken)
		{
			var result = await getInvokeAsync((TApi) api)((TRequestDto) request, cancellationToken).ConfigureAwait(false);
			if (result.IsFailure)
				return result.ToFailure();

			return ServiceResult.Success(Enumerate(result.Value, cancellationToken));

			static async IAsyncEnumerable<ServiceResult<ServiceDto>> Enumerate(IAsyncEnumerable<ServiceResult<TResponseDto>> enumerable, [EnumeratorCancellation] CancellationToken cancellationToken)
			{
				await foreach (var result in enumerable.WithCancellation(cancellationToken))
					yield return result.Cast<ServiceDto>();
			}
		}

		return new StandardServiceEventInfo(name, serviceName, InvokeAsync);
	}

	private sealed class StandardServiceEventInfo : IServiceEventInfo
	{
		public string Name { get; }

		public string ServiceName { get; }

		public Task<ServiceResult<IAsyncEnumerable<ServiceResult<ServiceDto>>>> InvokeAsync(object service, ServiceDto request, CancellationToken cancellationToken = default) =>
			m_invokeAsync(service, request, cancellationToken);

		internal StandardServiceEventInfo(string name, string serviceName, Func<object, ServiceDto, CancellationToken, Task<ServiceResult<IAsyncEnumerable<ServiceResult<ServiceDto>>>>> invokeAsync)
		{
			Name = name;
			ServiceName = serviceName;
			m_invokeAsync = invokeAsync;
		}

		private readonly Func<object, ServiceDto, CancellationToken, Task<ServiceResult<IAsyncEnumerable<ServiceResult<ServiceDto>>>>> m_invokeAsync;
	}
}
