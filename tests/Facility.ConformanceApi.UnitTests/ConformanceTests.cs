using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Facility.ConformanceApi.Http;
using Facility.ConformanceApi.Testing;
using Facility.Core.Http;
using NCrunch.Framework;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Facility.ConformanceApi.UnitTests
{
	public class ConformanceTests
	{
		[TestCaseSource(nameof(TestNames))]
		public async Task RunTest(string test)
		{
			IConformanceApi getApiForTest(string testName)
			{
				return new HttpClientConformanceApi(
					new HttpClientServiceSettings
					{
						BaseUri = new Uri("https://example.com/"),
						Aspects = new[] { FacilityTestClientAspect.Create(testName) },
						HttpClient = s_httpClient,
					});
			}

			var result = await new ConformanceApiTester(s_testProvider, getApiForTest).RunTestAsync(test, CancellationToken.None).ConfigureAwait(false);
			if (result.Status != ConformanceTestStatus.Pass)
				Assert.Fail(result.Message);
		}

		private static IConformanceTestProvider CreateTestProvider() =>
			new ConformanceTestProvider(Path.Combine(TestUtility.GetSolutionDirectory(), "conformance", "tests.json"));

		private static HttpClient CreateHttpClient()
		{
			IConformanceApi getApiForRequest(HttpRequestMessage httpRequest)
			{
				string testName = httpRequest.Headers.TryGetValues(FacilityTestClientAspect.HeaderName, out var values) ? string.Join(",", values) : null;
				return new ConformanceApiService(s_testProvider.TryGetTestInfo(testName));
			}

			var handler = new ConformanceApiHttpHandler(getApiForRequest, new ServiceHttpHandlerSettings()) { InnerHandler = new NotFoundHttpHandler() };
			return new HttpClient(handler);
		}

		private sealed class NotFoundHttpHandler : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
				Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
		}

		private static readonly IConformanceTestProvider s_testProvider = CreateTestProvider();

		private static IReadOnlyList<string> TestNames => s_testProvider.GetTestNames();

		private static readonly HttpClient s_httpClient = CreateHttpClient();
	}
}
