using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core.Http
{
	/// <summary>
	/// Sets the Accept header of the request.
	/// </summary>
	[Obsolete("Use CommonClientAspects.RequestAccept.")]
	public sealed class AcceptClientAspect : HttpClientServiceAspect
	{
		/// <summary>
		/// Creates an aspect that sets the Accept header to the specified string.
		/// </summary>
		public static HttpClientServiceAspect Create(string acceptHeader)
		{
			return new AcceptClientAspect(acceptHeader);
		}

		/// <summary>
		/// Called right before the request is sent.
		/// </summary>
		protected override Task RequestReadyAsyncCore(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken)
		{
			if (m_acceptHeader != null)
				httpRequest.Headers.Accept.Add(m_acceptHeader);
			return Task.CompletedTask;
		}

		private AcceptClientAspect(string acceptHeader)
		{
			if (!string.IsNullOrWhiteSpace(acceptHeader))
				m_acceptHeader = MediaTypeWithQualityHeaderValue.Parse(acceptHeader);
		}

		private readonly MediaTypeWithQualityHeaderValue? m_acceptHeader;
	}
}
