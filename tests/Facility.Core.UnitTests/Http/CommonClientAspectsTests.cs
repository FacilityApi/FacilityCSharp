using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests.Http
{
	public class CommonClientAspectsTests
	{
		[Test]
		public async Task RequestAccept()
		{
			const string header = HttpServiceUtility.JsonMediaType;
			var aspect = CommonClientAspects.RequestAccept(header);
			var httpRequest = new HttpRequestMessage();
			await aspect.RequestReadyAsync(httpRequest, new TestDto(), CancellationToken.None);
			httpRequest.Headers.Accept.ToString().Should().Be(header);
		}

		[Test]
		public async Task RequestAuthorization()
		{
			const string header = "MyAuth Whatever";
			var aspect = CommonClientAspects.RequestAuthorization(header);
			var httpRequest = new HttpRequestMessage();
			await aspect.RequestReadyAsync(httpRequest, new TestDto(), CancellationToken.None);
			httpRequest.Headers.Authorization!.ToString().Should().Be(header);
		}

		[Test]
		public async Task RequestHeader()
		{
			const string headerName = "X-Adjective";
			const string headerValue = "shiny";
			var aspect = CommonClientAspects.RequestHeader(headerName, headerValue);
			var httpRequest = new HttpRequestMessage();
			await aspect.RequestReadyAsync(httpRequest, new TestDto(), CancellationToken.None);
			httpRequest.Headers.TryGetValues(headerName, out var values);
			values!.Single().Should().Be(headerValue);
		}

		[Test]
		public async Task RequestUserAgent()
		{
			const string header = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.1 Safari/537.36";
			var aspect = CommonClientAspects.RequestUserAgent(header);
			var httpRequest = new HttpRequestMessage();
			await aspect.RequestReadyAsync(httpRequest, new TestDto(), CancellationToken.None);
			httpRequest.Headers.UserAgent.ToString().Should().Be(header);
		}

		private class TestDto : ServiceDto<TestDto>
		{
			public override bool IsEquivalentTo(TestDto? other) => other != null;
		}
	}
}
