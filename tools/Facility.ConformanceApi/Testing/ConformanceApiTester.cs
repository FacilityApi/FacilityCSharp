using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Newtonsoft.Json.Linq;

namespace Facility.ConformanceApi.Testing
{
	/// <summary>
	/// Tests a conformance API service.
	/// </summary>
	public sealed class ConformanceApiTester
	{
		/// <summary>
		/// Creates a tester.
		/// </summary>
		/// <param name="testProvider">Provides the conformance tests.</param>
		/// <param name="getApiForTest">Creates an API interface for the specified test.</param>
		public ConformanceApiTester(IConformanceTestProvider testProvider, Func<string, IConformanceApi> getApiForTest)
		{
			m_getApiForTest = getApiForTest;
			m_testProvider = testProvider;
		}

		/// <summary>
		/// The test names.
		/// </summary>
		public IReadOnlyList<string> GetTestNames() => m_testProvider.GetTestNames();

		/// <summary>
		/// Runs all tests.
		/// </summary>
		public async Task<ConformanceTestRun> RunAllTestsAsync(CancellationToken cancellationToken)
		{
			var results = new List<ConformanceTestResult>();

			foreach (var testName in GetTestNames())
				results.Add(await RunTestAsync(testName, cancellationToken).ConfigureAwait(false));

			return new ConformanceTestRun(results);
		}

		/// <summary>
		/// Runs the test with the specified name.
		/// </summary>
		public async Task<ConformanceTestResult> RunTestAsync(string testName, CancellationToken cancellationToken)
		{
			try
			{
				ConformanceTestResult failure(string message) => new ConformanceTestResult(testName, ConformanceTestStatus.Fail, message);

				var testInfo = m_testProvider.TryGetTestInfo(testName);

				var api = m_getApiForTest(testName);

				string capitalize(string value) => value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);
				var methodInfo = typeof(IConformanceApi).GetMethod(capitalize(testInfo.Method) + "Async", BindingFlags.Public | BindingFlags.Instance);
				if (methodInfo == null)
					return failure($"Missing API method for {testInfo.Method}");

				var requestJObject = testInfo.Request;
				var requestDto = ServiceJsonUtility.FromJToken(requestJObject, methodInfo.GetParameters()[0].ParameterType);
				var requestRoundTripJObject = ServiceJsonUtility.ToJToken(requestDto);
				if (!JToken.DeepEquals(requestJObject, requestRoundTripJObject))
					return failure($"Request round trip failed. expected={ServiceJsonUtility.ToJson(requestJObject)} actual={ServiceJsonUtility.ToJson(requestRoundTripJObject)}");

				var task = (Task) methodInfo.Invoke(api, new[] { requestDto, cancellationToken });
				await task.ConfigureAwait(false);

				dynamic result = ((dynamic) task).Result;
				ServiceDto actualResponseDto = (ServiceDto) result.GetValueOrDefault();
				var expectedResponseJObject = testInfo.Response;
				var expectedErrorJObject = testInfo.Error;
				if (actualResponseDto != null)
				{
					var actualResponseJObject = (JObject) ServiceJsonUtility.ToJToken(actualResponseDto);

					if (expectedErrorJObject != null)
						return failure($"Got valid response; expected error. expected={ServiceJsonUtility.ToJson(expectedErrorJObject)} actual={ServiceJsonUtility.ToJson(actualResponseJObject)}");
					if (!JToken.DeepEquals(expectedResponseJObject, actualResponseJObject))
						return failure($"Response JSON did not match. expected={ServiceJsonUtility.ToJson(expectedResponseJObject)} actual={ServiceJsonUtility.ToJson(actualResponseJObject)}");
					var responseType = methodInfo.ReturnType.GetGenericArguments()[0].GetGenericArguments()[0];
					var expectedResponseDto = (ServiceDto) ServiceJsonUtility.FromJToken(expectedResponseJObject, responseType);
					if (!expectedResponseDto.IsEquivalentTo(actualResponseDto))
						return failure($"Response DTO did not match. expected={expectedResponseDto} actual={ServiceJsonUtility.ToJson(actualResponseDto)}");
				}
				else
				{
					ServiceErrorDto actualErrorDto = result.Error;
					var actualErrorJObject = (JObject) ServiceJsonUtility.ToJToken(actualErrorDto);

					if (expectedErrorJObject == null)
						return failure($"Got error; expected valid response. expected={ServiceJsonUtility.ToJson(expectedResponseJObject)} actual={ServiceJsonUtility.ToJson(actualErrorJObject)}");
					if (!JToken.DeepEquals(expectedErrorJObject, actualErrorJObject))
						return failure($"Error JSON did not match. expected={ServiceJsonUtility.ToJson(expectedErrorJObject)} actual={ServiceJsonUtility.ToJson(actualErrorJObject)}");
					var expectedErrorDto = ServiceJsonUtility.FromJToken<ServiceErrorDto>(expectedErrorJObject);
					if (!expectedErrorDto.IsEquivalentTo(actualErrorDto))
						return failure($"Error DTO did not match. expected={expectedErrorDto} actual={actualErrorDto}");
				}

				return new ConformanceTestResult(testName, ConformanceTestStatus.Pass);
			}
			catch (Exception exception)
			{
				return new ConformanceTestResult(testName, ConformanceTestStatus.Fail,
					$"Unhandled exception {exception.GetType().FullName}: {exception.Message}");
			}
		}

		private readonly IConformanceTestProvider m_testProvider;
		private readonly Func<string, IConformanceApi> m_getApiForTest;
	}
}
