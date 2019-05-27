using System;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Newtonsoft.Json.Linq;

namespace Facility.ConformanceApi.Testing
{
	/// <summary>
	/// Implements a conformance API service.
	/// </summary>
	public sealed class ConformanceApiService : IConformanceApi
	{
		/// <summary>
		/// Creates a service for the specified test.
		/// </summary>
		public ConformanceApiService(IConformanceTestProvider testProvider, string testName)
		{
			m_testProvider = testProvider ?? throw new ArgumentNullException(nameof(testProvider));
			m_testName = testName;
		}

		/// <inheritdoc />
		public Task<ServiceResult<GetApiInfoResponseDto>> GetApiInfoAsync(GetApiInfoRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<GetApiInfoResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<GetWidgetsResponseDto>> GetWidgetsAsync(GetWidgetsRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<GetWidgetsResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<CreateWidgetResponseDto>> CreateWidgetAsync(CreateWidgetRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<CreateWidgetResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<GetWidgetResponseDto>> GetWidgetAsync(GetWidgetRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<GetWidgetResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<DeleteWidgetResponseDto>> DeleteWidgetAsync(DeleteWidgetRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<DeleteWidgetResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<GetWidgetBatchResponseDto>> GetWidgetBatchAsync(GetWidgetBatchRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<GetWidgetBatchResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<MirrorFieldsResponseDto>> MirrorFieldsAsync(MirrorFieldsRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<MirrorFieldsResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<CheckQueryResponseDto>> CheckQueryAsync(CheckQueryRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<CheckQueryResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<CheckPathResponseDto>> CheckPathAsync(CheckPathRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<CheckPathResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<MirrorHeadersResponseDto>> MirrorHeadersAsync(MirrorHeadersRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<MirrorHeadersResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<MixedResponseDto>> MixedAsync(MixedRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<MixedResponseDto>(request));

		private ServiceResult<T> Execute<T>(ServiceDto request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			if (m_testName == null)
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest("Facility test name is missing; set the FacilityTest HTTP header."));

			var testInfo = m_testProvider.TryGetTestInfo(m_testName);
			if (testInfo == null)
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"Unknown Facility test: {m_testName}"));

			string uncapitalize(string value) => value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
			string methodName = uncapitalize(request.GetType().Name.Substring(0, request.GetType().Name.Length - "RequestDto".Length));
			if (methodName != testInfo.Method)
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"Unexpected method name for test {testInfo.Test}. expected={testInfo.Method} actual={methodName}"));

			var actualRequest = (JObject) ServiceJsonUtility.ToJToken(request);
			if (!JToken.DeepEquals(testInfo.Request, actualRequest))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"Request did not match for test {testInfo.Test}. expected={ServiceJsonUtility.ToJson(testInfo.Request)} actual={ServiceJsonUtility.ToJson(actualRequest)}"));

			if (testInfo.Error != null)
			{
				var error = ServiceJsonUtility.FromJToken<ServiceErrorDto>(testInfo.Error);
				var errorRoundTrip = ServiceJsonUtility.ToJToken(error);
				if (!JToken.DeepEquals(testInfo.Error, errorRoundTrip))
					return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"Error round trip failed for test {testInfo.Test}. expected={ServiceJsonUtility.ToJson(testInfo.Error)} actual={ServiceJsonUtility.ToJson(errorRoundTrip)}"));
				return ServiceResult.Failure(error);
			}
			else
			{
				var response = ServiceJsonUtility.FromJToken<T>(testInfo.Response);
				var responseRoundTrip = ServiceJsonUtility.ToJToken(response);
				if (!JToken.DeepEquals(testInfo.Response, responseRoundTrip))
					return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"Response round trip failed for test {testInfo.Test}. expected={ServiceJsonUtility.ToJson(testInfo.Response)} actual={ServiceJsonUtility.ToJson(responseRoundTrip)}"));
				return ServiceResult.Success(response);
			}
		}

		private readonly IConformanceTestProvider m_testProvider;
		private readonly string m_testName;
	}
}
