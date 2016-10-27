using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core.Http
{
	/// <summary>
	/// Sets the Authorization header of the request.
	/// </summary>
	public sealed class AuthorizationClientAspect : HttpClientServiceAspect
	{
		/// <summary>
		/// Creates an aspect that sets the Authorization header to the specified string.
		/// </summary>
		public AuthorizationClientAspect(string authorizationHeader)
		{
			m_authorizationHeader = authorizationHeader;
		}

		/// <summary>
		/// Called right before the request is sent.
		/// </summary>
		protected override Task RequestReadyAsyncCore(HttpRequestMessage request, ServiceDto requestDto, CancellationToken cancellationToken)
		{
			if (!string.IsNullOrWhiteSpace(m_authorizationHeader))
				request.Headers.Authorization = AuthenticationHeaderValue.Parse(m_authorizationHeader);
			return Task.FromResult<object>(null);
		}

		readonly string m_authorizationHeader;
	}
}
