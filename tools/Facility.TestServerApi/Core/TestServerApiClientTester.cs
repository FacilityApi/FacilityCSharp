using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Facility.Core.Http;
using Facility.ServerTesting;
using Facility.TestServerApi.Http;
using Newtonsoft.Json.Linq;

namespace Facility.TestServerApi.Core
{
	public sealed class TestServerApiClientTester
	{
		public TestServerApiClientTester(HttpClient httpClient)
		{
			m_httpClient = httpClient;
		}

		public async Task<ServerTestResult> GetApiInfo([CallerMemberName] string testName = null)
		{
			var client = new HttpClientTestServerApi(new HttpClientServiceSettings
			{
				HttpClient = m_httpClient,
				BaseUri = new Uri("http://example.com/"),
				Aspects = new[] { FacilityTestClientAspect.Create(testName) },
			});

			var request = new GetApiInfoRequestDto();
			var response = await client.GetApiInfoAsync(request, CancellationToken.None);
			var responseAsJObject = (JObject) ServiceJsonUtility.ToJToken(response);
			var finishTestResult = await client.FinishTestAsync(new FinishTestRequestDto { Response = responseAsJObject }, CancellationToken.None);
			return finishTestResult.IsSuccess ?
				new ServerTestResult(testName, MapTestStatus(finishTestResult.Value.Status), finishTestResult.Value.Message) :
				new ServerTestResult(testName, ServerTestStatus.Fail, finishTestResult.Error.Message);
		}

		private ServerTestStatus MapTestStatus(TestStatus? valueStatus) =>
			valueStatus == TestStatus.Pass ? ServerTestStatus.Pass : ServerTestStatus.Fail;

		private readonly HttpClient m_httpClient;
	}
}
