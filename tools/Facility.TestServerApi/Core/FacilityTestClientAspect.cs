using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Facility.Core.Http;

namespace Facility.TestServerApi.Core
{
	public sealed class FacilityTestClientAspect : HttpClientServiceAspect
	{
		/// <summary>
		/// Creates an aspect that sets the User-Agent header to the specified string.
		/// </summary>
		public static HttpClientServiceAspect Create(string testName)
		{
			return new FacilityTestClientAspect(testName);
		}

		/// <summary>
		/// Called right before the request is sent.
		/// </summary>
		protected override Task RequestReadyAsyncCore(HttpRequestMessage request, ServiceDto requestDto, CancellationToken cancellationToken)
		{
			if (!string.IsNullOrWhiteSpace(m_testName))
				request.Headers.Add("FacilityTest", m_testName);
			return Task.FromResult(default(object));
		}

		private FacilityTestClientAspect(string testName)
		{
			m_testName = testName;
		}

		readonly string m_testName;
	}
}
