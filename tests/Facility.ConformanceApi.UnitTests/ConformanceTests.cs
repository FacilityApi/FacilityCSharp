using System.Net;
using Facility.ConformanceApi.Http;
using Facility.ConformanceApi.Testing;
using Facility.Core;
using Facility.Core.Http;
using NUnit.Framework;

namespace Facility.ConformanceApi.UnitTests;

[TestFixtureSource(nameof(ServiceSerializers))]
public sealed class ConformanceTests : ServiceSerializerTestsBase, IDisposable
{
	public ConformanceTests(ServiceSerializer serializer)
		: base(serializer)
	{
		m_tests = CreateTestProvider(JsonSerializer);
		m_contentSerializer = HttpContentSerializer.Create(Serializer).WithMemoryStreamCreator(MemoryStreamManager.GetStream);

		var handler = new ConformanceApiHttpHandler(
				service: new ConformanceApiService(new ConformanceApiServiceSettings { Tests = m_tests, JsonSerializer = JsonSerializer }),
				settings: new ServiceHttpHandlerSettings { ContentSerializer = m_contentSerializer })
			{ InnerHandler = new NotFoundHttpHandler() };
		m_httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://example.com/") };
	}

	[TestCaseSource(nameof(TestNames))]
	public async Task RunTest(string testName)
	{
		var settings = new ConformanceApiTesterSettings
		{
			Tests = m_tests,
			Api = new HttpClientConformanceApi(new HttpClientServiceSettings { HttpClient = m_httpClient, ContentSerializer = m_contentSerializer }),
			JsonSerializer = JsonSerializer,
			HttpClient = m_httpClient,
		};
		var test = m_tests.Single(x => x.Test == testName);
		var result = await new ConformanceApiTester(settings).RunTestAsync(test).ConfigureAwait(false);
		if (result.Status != ConformanceTestStatus.Pass)
			Assert.Fail(result.Message);
	}

	public void Dispose() => m_httpClient.Dispose();

	private static IReadOnlyList<ConformanceTestInfo> CreateTestProvider(JsonServiceSerializer jsonSerializer)
	{
		using var testsJsonReader = new StreamReader(typeof(ConformanceTests).Assembly.GetManifestResourceStream("Facility.ConformanceApi.UnitTests.ConformanceTests.json")!);
		return ConformanceTestsInfo.FromJson(testsJsonReader.ReadToEnd(), jsonSerializer).Tests!;
	}

	private sealed class NotFoundHttpHandler : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
	}

	private static IReadOnlyList<string> TestNames { get; } = CreateTestProvider(NewtonsoftJsonServiceSerializer.Instance).Select(x => x.Test!).ToList();

	private readonly IReadOnlyList<ConformanceTestInfo> m_tests;
	private readonly HttpClient m_httpClient;
	private HttpContentSerializer m_contentSerializer;
}
