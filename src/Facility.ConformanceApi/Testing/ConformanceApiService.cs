using Facility.Core;

namespace Facility.ConformanceApi.Testing;

/// <summary>
/// Implements a conformance API service.
/// </summary>
public sealed class ConformanceApiService : IConformanceApi
{
	/// <summary>
	/// Creates a service for the specified test.
	/// </summary>
	public ConformanceApiService(ConformanceApiServiceSettings settings)
	{
		_ = settings ?? throw new ArgumentNullException(nameof(settings));
		m_tests = settings.Tests ?? throw new ArgumentException($"{nameof(settings.Tests)} is required.", nameof(settings));
		m_jsonSerializer = settings.JsonSerializer ?? throw new ArgumentException($"{nameof(settings.JsonSerializer)} is required.", nameof(settings));
	}

	/// <summary>
	/// Creates a service for the specified tests.
	/// </summary>
	[Obsolete("Use settings overload.")]
	public ConformanceApiService(IReadOnlyList<ConformanceTestInfo> tests)
		: this(new ConformanceApiServiceSettings { Tests = tests, JsonSerializer = NewtonsoftJsonServiceSerializer.Instance })
	{
	}

	/// <inheritdoc />
	public Task<ServiceResult<GetApiInfoResponseDto>> GetApiInfoAsync(GetApiInfoRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<GetApiInfoResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<GetWidgetsResponseDto>> GetWidgetsAsync(GetWidgetsRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<GetWidgetsResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<CreateWidgetResponseDto>> CreateWidgetAsync(CreateWidgetRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<CreateWidgetResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<GetWidgetResponseDto>> GetWidgetAsync(GetWidgetRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<GetWidgetResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<DeleteWidgetResponseDto>> DeleteWidgetAsync(DeleteWidgetRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<DeleteWidgetResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<GetWidgetBatchResponseDto>> GetWidgetBatchAsync(GetWidgetBatchRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<GetWidgetBatchResponseDto>(request));

	public Task<ServiceResult<GetExternalWidgetsResponseDto>> GetExternalWidgetsAsync(GetExternalWidgetsRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<GetExternalWidgetsResponseDto>(request));

	public Task<ServiceResult<CreateExternalWidgetResponseDto>> CreateExternalWidgetAsync(CreateExternalWidgetRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<CreateExternalWidgetResponseDto>(request));

	public Task<ServiceResult<GetExternalWidgetResponseDto>> GetExternalWidgetAsync(GetExternalWidgetRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<GetExternalWidgetResponseDto>(request));

	public Task<ServiceResult<DeleteExternalWidgetResponseDto>> DeleteExternalWidgetAsync(DeleteExternalWidgetRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<DeleteExternalWidgetResponseDto>(request));

	public Task<ServiceResult<GetExternalWidgetBatchResponseDto>> GetExternalWidgetBatchAsync(GetExternalWidgetBatchRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<GetExternalWidgetBatchResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<MirrorFieldsResponseDto>> MirrorFieldsAsync(MirrorFieldsRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<MirrorFieldsResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<CheckQueryResponseDto>> CheckQueryAsync(CheckQueryRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<CheckQueryResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<CheckPathResponseDto>> CheckPathAsync(CheckPathRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<CheckPathResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<MirrorHeadersResponseDto>> MirrorHeadersAsync(MirrorHeadersRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<MirrorHeadersResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<MixedResponseDto>> MixedAsync(MixedRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<MixedResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<RequiredResponseDto>> RequiredAsync(RequiredRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<RequiredResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<MirrorBytesResponseDto>> MirrorBytesAsync(MirrorBytesRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<MirrorBytesResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<MirrorTextResponseDto>> MirrorTextAsync(MirrorTextRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<MirrorTextResponseDto>(request));

	/// <inheritdoc />
	public Task<ServiceResult<BodyTypesResponseDto>> BodyTypesAsync(BodyTypesRequestDto request, CancellationToken cancellationToken = default) =>
		Task.FromResult(Execute<BodyTypesResponseDto>(request));

	private ServiceResult<T> Execute<T>(ServiceDto request)
	{
		if (request == null)
			throw new ArgumentNullException(nameof(request));

		var methodName = Uncapitalize(request.GetType().Name.Substring(0, request.GetType().Name.Length - "RequestDto".Length));
		var testsWithMethodName = m_tests.Where(x => x.Method == methodName).ToList();
		if (testsWithMethodName.Count == 0)
			return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"No tests found for method {methodName}."));

		var actualRequest = m_jsonSerializer.ToServiceObject(request);
		var testsWithMatchingRequest = testsWithMethodName.Where(x => ServiceObjectUtility.DeepEquals(x.Request, actualRequest)).ToList();
		if (testsWithMatchingRequest.Count != 1)
		{
			return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest(
				$"{testsWithMatchingRequest.Count} of {testsWithMethodName.Count} tests for method {methodName} matched request: " +
				$"{m_jsonSerializer.ToJson(actualRequest)} ({string.Join(", ", testsWithMethodName.Select(x => m_jsonSerializer.ToJson(x.Request)))})"));
		}
		var testInfo = testsWithMatchingRequest[0];

		if (testInfo.Error != null)
		{
			var error = m_jsonSerializer.FromServiceObject<ServiceErrorDto>(testInfo.Error);
			var errorRoundTrip = m_jsonSerializer.ToServiceObject(error);
			if (!ServiceObjectUtility.DeepEquals(testInfo.Error, errorRoundTrip))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"Error round trip failed for test {testInfo.Test}. expected={m_jsonSerializer.ToJson(testInfo.Error)} actual={m_jsonSerializer.ToJson(errorRoundTrip)}"));
			return ServiceResult.Failure(error);
		}
		else
		{
			var response = m_jsonSerializer.FromServiceObject<T>(testInfo.Response!);
			var responseRoundTrip = m_jsonSerializer.ToServiceObject(response);
			if (!ServiceObjectUtility.DeepEquals(testInfo.Response, responseRoundTrip))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"Response round trip failed for test {testInfo.Test}. expected={m_jsonSerializer.ToJson(testInfo.Response)} actual={m_jsonSerializer.ToJson(responseRoundTrip)}"));
			return ServiceResult.Success(response);
		}
	}

	private static string Uncapitalize(string value) => value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);

	private readonly IReadOnlyList<ConformanceTestInfo> m_tests;
	private readonly JsonServiceSerializer m_jsonSerializer;
}
