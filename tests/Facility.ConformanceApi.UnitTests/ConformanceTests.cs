using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Facility.ConformanceApi.Http;
using Facility.ConformanceApi.Testing;
using Facility.Core.Http;
using NUnit.Framework;

namespace Facility.ConformanceApi.UnitTests
{
	public class ConformanceTests
	{
		[TestCaseSource(nameof(TestNames))]
		public async Task RunTest(string testName)
		{
			var api = new HttpClientConformanceApi(new HttpClientServiceSettings { HttpClient = s_httpClient });
			var test = s_tests.Single(x => x.Test == testName);
			var result = await new ConformanceApiTester(s_tests, api).RunTestAsync(test, CancellationToken.None).ConfigureAwait(false);
			if (result.Status != ConformanceTestStatus.Pass)
				Assert.Fail(result.Message);
		}

		private static IReadOnlyList<ConformanceTestInfo> CreateTestProvider()
		{
			using var testsJsonReader = new StreamReader(typeof(ConformanceTests).Assembly.GetManifestResourceStream("Facility.ConformanceApi.UnitTests.ConformanceTests.json"));
			return ConformanceTestsInfo.FromJson(testsJsonReader.ReadToEnd()).Tests!;
		}

		private static HttpClient CreateHttpClient()
		{
			var handler = new ConformanceApiHttpHandler(
				service: new ConformanceApiService(s_tests),
				settings: new ServiceHttpHandlerSettings())
			{ InnerHandler = new NotFoundHttpHandler() };
			return new HttpClient(handler) { BaseAddress = new Uri("http://example.com/") };
		}

		private sealed class NotFoundHttpHandler : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
				Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
		}

		private static readonly IReadOnlyList<ConformanceTestInfo> s_tests = CreateTestProvider();

		private static IReadOnlyList<string> TestNames => s_tests.Select(x => x.Test!).ToList();

		private static readonly HttpClient s_httpClient = CreateHttpClient();
	}
}
