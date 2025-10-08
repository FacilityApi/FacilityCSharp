#if NET472
using System.Net.Http;
#endif
using Facility.Core.Assertions;
using Facility.Core.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests.Http;

internal sealed class HttpClientServiceTests : HttpClientService
{
	public HttpClientServiceTests()
		: base(new HttpClientServiceSettings() { BaseUri = new Uri("http://localhost/"), HttpClient = s_httpClient }, new HttpClientServiceDefaults())
	{
	}

	[OneTimeTearDown]
	public void OneTimeTearDown() => s_httpClient.Dispose();

	[Test]
	public async Task HttpClientPropertyTest()
	{
		HttpClient.Should().NotBeNull();
		HttpClient.Should().BeSameAs(s_httpClient);
	}

	private static readonly HttpClient s_httpClient = new HttpClient();
}
