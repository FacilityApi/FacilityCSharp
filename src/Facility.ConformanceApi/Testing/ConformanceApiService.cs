using System;
using System.Collections.Generic;
using System.Linq;
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
		public ConformanceApiService(IReadOnlyList<ConformanceTestInfo> tests)
		{
			m_tests = tests ?? throw new ArgumentNullException(nameof(tests));
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

		private ServiceResult<T> Execute<T>(ServiceDto request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			string uncapitalize(string value) => value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
			string methodName = uncapitalize(request.GetType().Name.Substring(0, request.GetType().Name.Length - "RequestDto".Length));
			var testsWithMethodName = m_tests.Where(x => x.Method == methodName).ToList();
			if (testsWithMethodName.Count == 0)
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"No tests found for method {methodName}."));

			var actualRequest = (JObject) ServiceJsonUtility.ToJToken(request);
			var testsWithMatchingRequest = testsWithMethodName.Where(x => JToken.DeepEquals(x.Request, actualRequest)).ToList();
			if (testsWithMatchingRequest.Count != 1)
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"{testsWithMatchingRequest.Count} of {testsWithMethodName.Count} tests for method {methodName} matched request: {ServiceJsonUtility.ToJson(actualRequest)}"));
			var testInfo = testsWithMatchingRequest[0];

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

		private readonly IReadOnlyList<ConformanceTestInfo> m_tests;
	}
}
