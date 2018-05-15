using System.Net.Http;
using System.Threading.Tasks;
using Facility.ServerTesting;
using Facility.TestServerApi.Core;
using FluentAssertions;
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
			var result = await tester.GetApiInfo();
			if (result.Status != ServerTestStatus.Pass)
				throw new TestFailedException(result.Message);
		}
	}
}
