using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core.Http
{
	/// <summary>
	/// Sets the Authorization header of the request.
	/// </summary>
	[Obsolete("Use CommonClientAspects.RequestAuthorization.")]
	public sealed class AuthorizationClientAspect : HttpClientServiceAspect
	{
		/// <summary>
		/// Creates an aspect that sets the Authorization header to the specified string.
		/// </summary>
		public static HttpClientServiceAspect Create(string authorizationHeader)
		{
			return new AuthorizationClientAspect(authorizationHeader);
		}

		/// <summary>
		/// Called right before the request is sent.
		/// </summary>
		protected override Task RequestReadyAsyncCore(HttpRequestMessage request, ServiceDto requestDto, CancellationToken cancellationToken)
		{
			if (m_authorizationHeader != null)
				request.Headers.Authorization = m_authorizationHeader;
			return Task.CompletedTask;
		}

		private AuthorizationClientAspect(string authorizationHeader)
		{
			if (!string.IsNullOrWhiteSpace(authorizationHeader))
				m_authorizationHeader = AuthenticationHeaderValue.Parse(authorizationHeader);
		}

		private readonly AuthenticationHeaderValue? m_authorizationHeader;
	}
}
