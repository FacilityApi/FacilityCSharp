namespace Facility.Core;

/// <summary>
/// Helpers for service method information.
/// </summary>
public static class ServiceMethodInfo
{
	/// <summary>
	/// Creates service method information.
	/// </summary>
	/// <remarks>For internal use by generated code.</remarks>
	public static IServiceMethodInfo Create<TApi, TRequestDto, TResponseDto>(string name, string serviceName, Func<TApi, Func<TRequestDto, CancellationToken, Task<ServiceResult<TResponseDto>>>> getInvokeAsync)
		where TRequestDto : ServiceDto
		where TResponseDto : ServiceDto
	{
		async Task<ServiceResult<ServiceDto>> InvokeAsync(object api, ServiceDto request, CancellationToken cancellationToken) =>
			(await getInvokeAsync((TApi) api)((TRequestDto) request, cancellationToken).ConfigureAwait(false)).Cast<ServiceDto>();

		return new StandardServiceMethodInfo(name, serviceName, InvokeAsync);
	}

	private sealed class StandardServiceMethodInfo : IServiceMethodInfo
	{
		public string Name { get; }

		public string ServiceName { get; }

		public Task<ServiceResult<ServiceDto>> InvokeAsync(object service, ServiceDto request, CancellationToken cancellationToken = default) =>
			m_invokeAsync(service, request, cancellationToken);

		internal StandardServiceMethodInfo(string name, string serviceName, Func<object, ServiceDto, CancellationToken, Task<ServiceResult<ServiceDto>>> invokeAsync)
		{
			Name = name;
			ServiceName = serviceName;
			m_invokeAsync = invokeAsync;
		}

		private readonly Func<object, ServiceDto, CancellationToken, Task<ServiceResult<ServiceDto>>> m_invokeAsync;
	}
}
