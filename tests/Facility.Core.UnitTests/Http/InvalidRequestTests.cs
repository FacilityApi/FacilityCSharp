using System.Net;
#if NET472
using System.Net.Http;
#endif
using System.Text;
using Facility.ConformanceApi;
using Facility.ConformanceApi.Http;
using Facility.Core.Assertions;
using Facility.Core.Http;
using NUnit.Framework;

namespace Facility.Core.UnitTests.Http;

internal sealed class InvalidRequestTests
{
	[Test]
	public async Task UriTooLong()
	{
		var api = CreateApi();
		var result = await api.GetWidgetsAsync(new GetWidgetsRequestDto { Query = new string('x', 100000) });
		result.Should().BeFailure(ServiceErrors.InternalError);
	}

	private IConformanceApi CreateApi()
	{
		var handler = new FakeHttpHandler();
		var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://example.com/") };
		return new HttpClientConformanceApi(new HttpClientServiceSettings { HttpClient = httpClient });
	}

	private sealed class FakeHttpHandler : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}", Encoding.UTF8, "application/json") });
	}
}
