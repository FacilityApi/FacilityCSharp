using System.Net;
using Facility.ConformanceApi.Http;
using Facility.ConformanceApi.Testing;
using Facility.Core;
using Facility.Core.Http;
using NUnit.Framework;

namespace Facility.ConformanceApi.UnitTests
{
	[TestFixtureSource(nameof(ServiceSerializers))]
	public class ConformanceTests : ServiceSerializerTestBase
	{
		public ConformanceTests(ServiceSerializer serializer)
			: base(serializer)
		{
			m_tests = CreateTestProvider(Serializer);
			m_httpClient = CreateHttpClient(m_tests, Serializer);
		}

		[TestCaseSource(nameof(TestNames))]
		public async Task RunTest(string testName)
		{
			var api = new HttpClientConformanceApi(new HttpClientServiceSettings { HttpClient = m_httpClient });
			var test = m_tests.Single(x => x.Test == testName);
			var result = await new ConformanceApiTester(m_tests, api, m_httpClient, Serializer).RunTestAsync(test).ConfigureAwait(false);
			if (result.Status != ConformanceTestStatus.Pass)
				Assert.Fail(result.Message);
		}

		private static IReadOnlyList<ConformanceTestInfo> CreateTestProvider(ServiceSerializer serializer)
		{
			using var testsJsonReader = new StreamReader(typeof(ConformanceTests).Assembly.GetManifestResourceStream("Facility.ConformanceApi.UnitTests.ConformanceTests.json")!);
			return ConformanceTestsInfo.FromJson(testsJsonReader.ReadToEnd(), serializer).Tests!;
		}

		private static HttpClient CreateHttpClient(IReadOnlyList<ConformanceTestInfo> tests, ServiceSerializer serviceSerializer)
		{
			var handler = new ConformanceApiHttpHandler(
					service: new ConformanceApiService(tests, serviceSerializer),
					settings: new ServiceHttpHandlerSettings())
			{ InnerHandler = new NotFoundHttpHandler() };
			return new HttpClient(handler) { BaseAddress = new Uri("http://example.com/") };
		}

		private sealed class NotFoundHttpHandler : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
				Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
		}

		private static IReadOnlyList<string> TestNames { get; } = CreateTestProvider(ServiceSerializer.Default).Select(x => x.Test!).ToList();

		private readonly IReadOnlyList<ConformanceTestInfo> m_tests;
		private readonly HttpClient m_httpClient;
	}
}
