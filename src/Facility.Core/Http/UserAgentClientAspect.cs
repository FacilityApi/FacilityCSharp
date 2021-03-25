using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core.Http
{
	/// <summary>
	/// Sets the User-Agent header of the request.
	/// </summary>
	[Obsolete("Use CommonClientAspects.RequestUserAgent.")]
	public sealed class UserAgentClientAspect : HttpClientServiceAspect
	{
		/// <summary>
		/// Creates an aspect that sets the User-Agent header to the specified string.
		/// </summary>
		public static HttpClientServiceAspect Create(string userAgent)
		{
			return new UserAgentClientAspect(userAgent);
		}

		/// <summary>
		/// Called right before the request is sent.
		/// </summary>
		protected override Task RequestReadyAsyncCore(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken)
		{
			if (!string.IsNullOrWhiteSpace(m_userAgent))
				httpRequest.Headers.Add("User-Agent", m_userAgent);
			return Task.CompletedTask;
		}

		private UserAgentClientAspect(string userAgent)
		{
			m_userAgent = userAgent;
		}

		private readonly string m_userAgent;
	}
}
