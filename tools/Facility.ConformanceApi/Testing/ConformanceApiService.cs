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
		public ConformanceApiService(ConformanceTestInfo testInfo)
		{
			m_testInfo = testInfo;
		}

		/// <inheritdoc />
		public Task<ServiceResult<GetApiInfoResponseDto>> GetApiInfoAsync(GetApiInfoRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<GetApiInfoResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<CreateWidgetResponseDto>> CreateWidgetAsync(CreateWidgetRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<CreateWidgetResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<GetWidgetResponseDto>> GetWidgetAsync(GetWidgetRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<GetWidgetResponseDto>(request));

		/// <inheritdoc />
		public Task<ServiceResult<DeleteWidgetResponseDto>> DeleteWidgetAsync(DeleteWidgetRequestDto request, CancellationToken cancellationToken) =>
			Task.FromResult(Execute<DeleteWidgetResponseDto>(request));

		private ServiceResult<T> Execute<T>(ServiceDto request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			string uncapitalize(string value) => value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
			string methodName = uncapitalize(request.GetType().Name.Substring(0, request.GetType().Name.Length - "RequestDto".Length));
			if (methodName != m_testInfo.Method)
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"Unexpected method name for test {m_testInfo.TestName}. expected={m_testInfo.Method} actual={methodName}"));

			var actualRequest = (JObject) ServiceJsonUtility.ToJToken(request);
			var expectedRequest = m_testInfo.Request ?? new JObject();
			if (!JToken.DeepEquals(expectedRequest, actualRequest))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest($"Request did not match for test {m_testInfo.TestName}. expected={ServiceJsonUtility.ToJson(expectedRequest)} actual={ServiceJsonUtility.ToJson(actualRequest)}"));

			if (m_testInfo.Error != null)
				return ServiceResult.Failure(ServiceJsonUtility.FromJToken<ServiceErrorDto>(m_testInfo.Error));
			else
				return ServiceResult.Success(ServiceJsonUtility.FromJToken<T>(m_testInfo.Response ?? new JObject()));
		}

		private readonly ConformanceTestInfo m_testInfo;
	}
}
