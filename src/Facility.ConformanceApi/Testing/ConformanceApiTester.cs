using System.Reflection;
using Facility.Core;

namespace Facility.ConformanceApi.Testing;

/// <summary>
/// Tests a conformance API service.
/// </summary>
public sealed class ConformanceApiTester
{
	/// <summary>
	/// Creates a tester.
	/// </summary>
	public ConformanceApiTester(ConformanceApiTesterSettings settings)
	{
		_ = settings ?? throw new ArgumentNullException(nameof(settings));
		m_tests = settings.Tests ?? throw new ArgumentException($"{nameof(settings.Tests)} is required.", nameof(settings));
		m_api = settings.Api ?? throw new ArgumentException($"{nameof(settings.Api)} is required.", nameof(settings));
		m_jsonSerializer = settings.JsonSerializer ?? throw new ArgumentException($"{nameof(settings.JsonSerializer)} is required.", nameof(settings));
		m_httpClient = settings.HttpClient;

		var sameNameTests = m_tests.GroupBy(x => x.Test).FirstOrDefault(x => x.Count() != 1);
		if (sameNameTests != null)
			throw new ArgumentException($"Multiple tests have the name {sameNameTests.First().Test}");

		foreach (var testsPerMethod in m_tests.GroupBy(x => x.Method).Select(x => x.ToList()))
		{
			for (var i = 0; i < testsPerMethod.Count; i++)
			{
				for (var j = i + 1; j < testsPerMethod.Count; j++)
				{
					if (ServiceObjectUtility.DeepEquals(testsPerMethod[i].Request, testsPerMethod[j].Request))
						throw new ArgumentException($"Tests must not have the same method name and request data, e.g. {testsPerMethod[i].Test} and {testsPerMethod[j].Test}.");
				}
			}
		}
	}

	/// <summary>
	/// Creates a tester.
	/// </summary>
	/// <param name="tests">The conformance tests.</param>
	/// <param name="api">The API interface to test.</param>
	[Obsolete("Use settings overload.")]
	public ConformanceApiTester(IReadOnlyList<ConformanceTestInfo> tests, IConformanceApi api)
		: this(new ConformanceApiTesterSettings { Tests = tests, Api = api, JsonSerializer = NewtonsoftJsonServiceSerializer.Instance })
	{
	}

	/// <summary>
	/// Creates a tester.
	/// </summary>
	/// <param name="tests">The conformance tests.</param>
	/// <param name="api">The API interface to test.</param>
	/// <param name="httpClient">The optional HTTP client for HTTP tests.</param>
	[Obsolete("Use settings overload.")]
	public ConformanceApiTester(IReadOnlyList<ConformanceTestInfo> tests, IConformanceApi api, HttpClient? httpClient)
		: this(new ConformanceApiTesterSettings { Tests = tests, Api = api, JsonSerializer = NewtonsoftJsonServiceSerializer.Instance, HttpClient = httpClient })
	{
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
		var testName = test.Test!;
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

			var requestServiceObject = test.Request;
			if (requestServiceObject is null)
				return Failure($"Missing request for {test.Method}");
			var requestDto = m_jsonSerializer.FromServiceObject(requestServiceObject, methodInfo.GetParameters()[0].ParameterType);
			var requestRoundTripServiceObject = m_jsonSerializer.ToServiceObject(requestDto);
			if (!ServiceObjectUtility.DeepEquals(requestServiceObject, requestRoundTripServiceObject))
				return Failure($"Request round trip failed. expected={m_jsonSerializer.ToJson(requestServiceObject)} actual={m_jsonSerializer.ToJson(requestRoundTripServiceObject)}");

			var task = (Task) methodInfo.Invoke(m_api, new[] { requestDto, cancellationToken });
			await task.ConfigureAwait(false);

			var result = ((dynamic) task).Result;
			var actualResponseDto = (ServiceDto?) result.GetValueOrDefault();
			var expectedResponseServiceObject = test.Response;
			var expectedErrorServiceObject = test.Error;
			if (actualResponseDto != null)
			{
				var actualResponseServiceObject = m_jsonSerializer.ToServiceObject(actualResponseDto);

				if (expectedErrorServiceObject != null)
					return Failure($"Got valid response; expected error. expected={m_jsonSerializer.ToJson(expectedErrorServiceObject)} actual={m_jsonSerializer.ToJson(actualResponseServiceObject)}");
				if (!ServiceObjectUtility.DeepEquals(expectedResponseServiceObject, actualResponseServiceObject))
					return Failure($"Response content did not match. expected={m_jsonSerializer.ToJson(expectedResponseServiceObject)} actual={m_jsonSerializer.ToJson(actualResponseServiceObject)}");
				var responseType = methodInfo.ReturnType.GetGenericArguments()[0].GetGenericArguments()[0];
				var expectedResponseDto = (ServiceDto) m_jsonSerializer.FromServiceObject(expectedResponseServiceObject, responseType)!;
				if (!expectedResponseDto.IsEquivalentTo(actualResponseDto))
					return Failure($"Response DTO did not match. expected={expectedResponseDto} actual={m_jsonSerializer.ToJson(actualResponseDto)}");
			}
			else
			{
				var actualErrorDto = (ServiceErrorDto) result.Error;
				var actualErrorServiceObject = m_jsonSerializer.ToServiceObject(actualErrorDto);

				if (expectedErrorServiceObject == null)
					return Failure($"Got error; expected valid response. expected={m_jsonSerializer.ToJson(expectedResponseServiceObject)} actual={m_jsonSerializer.ToJson(actualErrorServiceObject)}");
				if (!ServiceObjectUtility.DeepEquals(expectedErrorServiceObject, actualErrorServiceObject))
					return Failure($"Error content did not match. expected={m_jsonSerializer.ToJson(expectedErrorServiceObject)} actual={m_jsonSerializer.ToJson(actualErrorServiceObject)}");
				var expectedErrorDto = m_jsonSerializer.FromServiceObject<ServiceErrorDto>(expectedErrorServiceObject);
				if (!expectedErrorDto.IsEquivalentTo(actualErrorDto))
					return Failure($"Error DTO did not match. expected={expectedErrorDto} actual={actualErrorDto}");
			}

			return new ConformanceTestResult(testName, ConformanceTestStatus.Pass);
		}
		catch (Exception exception)
		{
			return Failure($"Unhandled exception {exception.GetType().FullName}: {exception}");
		}
	}

	private readonly IReadOnlyList<ConformanceTestInfo> m_tests;
	private readonly IConformanceApi m_api;
	private readonly JsonServiceSerializer m_jsonSerializer;
	private readonly HttpClient? m_httpClient;
}
