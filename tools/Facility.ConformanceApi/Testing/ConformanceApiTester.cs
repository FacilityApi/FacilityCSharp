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

				var request = ServiceJsonUtility.FromJToken(testInfo.Request, methodInfo.GetParameters()[0].ParameterType);
				var requestRoundTrip = ServiceJsonUtility.ToJToken(request);
				if (!JToken.DeepEquals(testInfo.Request, requestRoundTrip))
					return failure($"Request round trip failed for test {testInfo.TestName}. expected={ServiceJsonUtility.ToJson(testInfo.Request)} actual={ServiceJsonUtility.ToJson(requestRoundTrip)}");

				var task = (Task) methodInfo.Invoke(api, new[] { request, cancellationToken });
				await task.ConfigureAwait(false);

				dynamic result = ((dynamic) task).Result;
				ServiceDto response = (ServiceDto) result.GetValueOrDefault();
				var expectedResponse = testInfo.Response;
				var expectedError = testInfo.Error;
				if (response != null)
				{
					var actualResponse = (JObject) ServiceJsonUtility.ToJToken(response);

					if (expectedError != null)
						return failure($"Got valid response; expected error. expected={ServiceJsonUtility.ToJson(expectedError)} actual={ServiceJsonUtility.ToJson(actualResponse)}");
					if (!JToken.DeepEquals(expectedResponse, actualResponse))
						return failure($"Response did not match. expected={ServiceJsonUtility.ToJson(expectedResponse)} actual={ServiceJsonUtility.ToJson(actualResponse)}");
				}
				else
				{
					ServiceErrorDto error = result.Error;
					var actualError = (JObject) ServiceJsonUtility.ToJToken(error);

					if (expectedError == null)
						return failure($"Got error; expected valid response. expected={ServiceJsonUtility.ToJson(expectedResponse)} actual={ServiceJsonUtility.ToJson(actualError)}");
					if (!JToken.DeepEquals(expectedError, actualError))
						return failure($"Error did not match. expected={ServiceJsonUtility.ToJson(expectedError)} actual={ServiceJsonUtility.ToJson(actualError)}");
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
