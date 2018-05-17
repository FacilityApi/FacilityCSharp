using System.Net.Http;
using System.Threading.Tasks;
using Facility.ServerTesting;
using Facility.TestServerApi.Core;
using NUnit.Framework;

namespace Facility.TestServerApi.UnitTests
{
	public class ClientUnitTests
	{
		[Test]
		public async Task GetApiInfo()
		{
			var httpClient = new HttpClient(new ClientTestingHttpHandler());
			var tester = new TestServerApiClientTester(httpClient);
			var result = await tester.GetApiInfo(nameof(GetApiInfo));
			if (result.Status != ServerTestStatus.Pass)
				throw new TestFailedException(result.Message);
		}

		[Test]
		public async Task CreateWidget()
		{
			var httpClient = new HttpClient(new ClientTestingHttpHandler());
			var tester = new TestServerApiClientTester(httpClient);
			var result = await tester.CreateWidget(nameof(CreateWidget));
			if (result.Status != ServerTestStatus.Pass)
				throw new TestFailedException(result.Message);
		}
	}
}
