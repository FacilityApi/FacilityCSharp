using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
		/// <param name="tests">The conformance tests.</param>
		/// <param name="api">The API interface to test.</param>
		public ConformanceApiTester(IReadOnlyList<ConformanceTestInfo> tests, IConformanceApi api)
			: this(tests, api, httpClient: null)
		{
		}

		/// <summary>
		/// Creates a tester.
		/// </summary>
		/// <param name="tests">The conformance tests.</param>
		/// <param name="api">The API interface to test.</param>
		/// <param name="httpClient">The optional HTTP client for HTTP tests.</param>
		public ConformanceApiTester(IReadOnlyList<ConformanceTestInfo> tests, IConformanceApi api, HttpClient? httpClient)
		{
			m_tests = tests ?? throw new ArgumentNullException(nameof(tests));
			m_api = api ?? throw new ArgumentNullException(nameof(api));
			m_httpClient = httpClient;

			var sameNameTests = m_tests.GroupBy(x => x.Test).FirstOrDefault(x => x.Count() != 1);
			if (sameNameTests != null)
				throw new ArgumentException($"Multiple tests have the name {sameNameTests.First().Test}");

			foreach (var testsPerMethod in m_tests.GroupBy(x => x.Method).Select(x => x.ToList()))
			{
				for (var i = 0; i < testsPerMethod.Count; i++)
				{
					for (var j = i + 1; j < testsPerMethod.Count; j++)
					{
						if (JToken.DeepEquals(testsPerMethod[i].Request, testsPerMethod[j].Request))
							throw new ArgumentException($"Tests must not have the same method name and request data, e.g. {testsPerMethod[i].Test} and {testsPerMethod[j].Test}.");
					}
				}
			}
		}

		/// <summary>
		/// Runs all tests.
		/// </summary>
		public async Task<ConformanceTestRun> RunAllTestsAsync(CancellationToken cancellationToken = default)
		{
			var results = new List<ConformanceTestResult>();

			foreach (var test in m_tests)
				results.Add(await RunTestAsync(test, cancellationToken).ConfigureAwait(false));

			return new ConformanceTestRun(results);
		}

		/// <summary>
		/// Runs the test with the specified name.
		/// </summary>
		public async Task<ConformanceTestResult> RunTestAsync(ConformanceTestInfo test, CancellationToken cancellationToken = default)
		{
			string testName = test.Test!;
			ConformanceTestResult Failure(string message) => new ConformanceTestResult(testName, ConformanceTestStatus.Fail, message);

			try
			{
				if (m_httpClient != null && test.HttpRequest != null)
				{
					if (test.HttpRequest.Method is null)
						return Failure("HTTP request missing method.");
					if (test.HttpRequest.Path is null)
						return Failure("HTTP request missing path.");

					var httpRequest = new HttpRequestMessage(new HttpMethod(test.HttpRequest.Method), test.HttpRequest.Path);
					var httpResponse = await m_httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
					if (!httpResponse.IsSuccessStatusCode)
						return Failure($"Got {(int) httpResponse.StatusCode} {httpResponse.ReasonPhrase}: {await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false)}");

					return new ConformanceTestResult(testName, ConformanceTestStatus.Pass);
				}

				static string Capitalize(string value) => value.Substring(0, 1).ToUpperInvariant() + value.Substring(1);
				var methodInfo = typeof(IConformanceApi).GetMethod(Capitalize(test.Method!) + "Async", BindingFlags.Public | BindingFlags.Instance);
				if (methodInfo == null)
					return Failure($"Missing API method for {test.Method}");

				var requestJObject = test.Request;
				var requestDto = ServiceJsonUtility.FromJToken(requestJObject, methodInfo.GetParameters()[0].ParameterType);
				var requestRoundTripJObject = ServiceJsonUtility.ToJToken(requestDto);
				if (!JToken.DeepEquals(requestJObject, requestRoundTripJObject))
					return Failure($"Request round trip failed. expected={ServiceJsonUtility.ToJson(requestJObject)} actual={ServiceJsonUtility.ToJson(requestRoundTripJObject)}");

				var task = (Task) methodInfo.Invoke(m_api, new[] { requestDto, cancellationToken });
				await task.ConfigureAwait(false);

				dynamic result = ((dynamic) task).Result;
				var actualResponseDto = (ServiceDto?) result.GetValueOrDefault();
				var expectedResponseJObject = test.Response;
				var expectedErrorJObject = test.Error;
				if (actualResponseDto != null)
				{
					var actualResponseJObject = (JObject) ServiceJsonUtility.ToJToken(actualResponseDto);

					if (expectedErrorJObject != null)
						return Failure($"Got valid response; expected error. expected={ServiceJsonUtility.ToJson(expectedErrorJObject)} actual={ServiceJsonUtility.ToJson(actualResponseJObject)}");
					if (!JToken.DeepEquals(expectedResponseJObject, actualResponseJObject))
						return Failure($"Response JSON did not match. expected={ServiceJsonUtility.ToJson(expectedResponseJObject)} actual={ServiceJsonUtility.ToJson(actualResponseJObject)}");
					var responseType = methodInfo.ReturnType.GetGenericArguments()[0].GetGenericArguments()[0];
					var expectedResponseDto = (ServiceDto) ServiceJsonUtility.FromJToken(expectedResponseJObject, responseType)!;
					if (!expectedResponseDto.IsEquivalentTo(actualResponseDto))
						return Failure($"Response DTO did not match. expected={expectedResponseDto} actual={ServiceJsonUtility.ToJson(actualResponseDto)}");
				}
				else
				{
					var actualErrorDto = (ServiceErrorDto) result.Error;
					var actualErrorJObject = (JObject) ServiceJsonUtility.ToJToken(actualErrorDto);

					if (expectedErrorJObject == null)
						return Failure($"Got error; expected valid response. expected={ServiceJsonUtility.ToJson(expectedResponseJObject)} actual={ServiceJsonUtility.ToJson(actualErrorJObject)}");
					if (!JToken.DeepEquals(expectedErrorJObject, actualErrorJObject))
						return Failure($"Error JSON did not match. expected={ServiceJsonUtility.ToJson(expectedErrorJObject)} actual={ServiceJsonUtility.ToJson(actualErrorJObject)}");
					var expectedErrorDto = ServiceJsonUtility.FromJToken<ServiceErrorDto>(expectedErrorJObject);
					if (!expectedErrorDto.IsEquivalentTo(actualErrorDto))
						return Failure($"Error DTO did not match. expected={expectedErrorDto} actual={actualErrorDto}");
				}

				return new ConformanceTestResult(testName, ConformanceTestStatus.Pass);
			}
			catch (Exception exception)
			{
				return Failure($"Unhandled exception {exception.GetType().FullName}: {exception.Message}");
			}
		}

		private readonly IReadOnlyList<ConformanceTestInfo> m_tests;
		private readonly IConformanceApi m_api;
		private readonly HttpClient? m_httpClient;
	}
}
