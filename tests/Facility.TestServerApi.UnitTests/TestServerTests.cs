using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core.Http;
using Facility.ServerTesting;
using Facility.TestServerApi.Core;
using Facility.TestServerApi.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.TestServerApi.UnitTests
{
	public class TestServerTests
	{
		[TestCaseSource(nameof(TestNames))]
		public async Task RunTest(string test)
		{
			var service = new TestServerApiService();
			var handler = new TestServerApiHttpHandler(service, new ServiceHttpHandlerSettings())
			{
				InnerHandler = new NotFoundHttpHandler(),
			};
			var httpClient = new HttpClient(handler);
			var tester = new ServerTester("http://example.com/", httpClient);
			var result = await tester.RunTestAsync(test);
			result.Status.Should().Be(ServerTestStatus.Pass, result.Message);
		}

		public sealed class NotFoundHttpHandler : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
				Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
		}

		private static IReadOnlyList<string> TestNames => ServerTester.GetTestNames();
	}
}
