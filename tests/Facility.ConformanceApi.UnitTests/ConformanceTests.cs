using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Facility.ConformanceApi.Http;
using Facility.ConformanceApi.Testing;
using Facility.Core;
using Facility.Core.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.ConformanceApi.UnitTests;

[TestFixtureSource(nameof(ServiceSerializers))]
public sealed class ConformanceTests : ServiceSerializerTestsBase, IDisposable
{
	public ConformanceTests(ServiceSerializer serializer)
		: base(serializer)
	{
		m_tests = CreateTestProvider();
		m_contentSerializer = HttpContentSerializer.Create(Serializer);

		var service = new ConformanceApiService(new ConformanceApiServiceSettings { Tests = m_tests, JsonSerializer = JsonSerializer });
		var settings = new ServiceHttpHandlerSettings { ContentSerializer = m_contentSerializer };
		var handler = new ConformanceApiHttpHandler(service, settings) { InnerHandler = new NotFoundHttpHandler() };
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
			Assert.Fail(result.Message!);
	}

	[Test]
	public async Task DefaultMediaType()
	{
		var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/");
		var httpResponse = await m_httpClient.SendAsync(httpRequest).ConfigureAwait(false);
		httpResponse.EnsureSuccessStatusCode();
		(httpResponse.Content.Headers.ContentType?.MediaType).Should().Be(m_contentSerializer.DefaultMediaType);
	}

	[Test]
	public async Task AcceptAnything()
	{
		var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/");
		httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
		var httpResponse = await m_httpClient.SendAsync(httpRequest).ConfigureAwait(false);
		httpResponse.EnsureSuccessStatusCode();
		(httpResponse.Content.Headers.ContentType?.MediaType).Should().Be(m_contentSerializer.DefaultMediaType);
	}

	public void Dispose() => m_httpClient.Dispose();

	private static IReadOnlyList<ConformanceTestInfo> CreateTestProvider()
	{
		using var testsJsonReader = new StreamReader(typeof(ConformanceTests).Assembly.GetManifestResourceStream("Facility.ConformanceApi.UnitTests.ConformanceTests.json")!);
		return ConformanceTestsInfo.FromJson(testsJsonReader.ReadToEnd()).Tests!;
	}

	private sealed class NotFoundHttpHandler : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
	}

	private static IReadOnlyList<string> TestNames { get; } = CreateTestProvider().Select(x => x.Test!).ToList();

	private readonly IReadOnlyList<ConformanceTestInfo> m_tests;
	private readonly HttpClient m_httpClient;
	private readonly HttpContentSerializer m_contentSerializer;
}
