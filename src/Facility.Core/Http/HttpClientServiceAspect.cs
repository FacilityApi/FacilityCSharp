using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core.Http
{
	/// <summary>
	/// Used to provide common functionality to every HTTP client request.
	/// </summary>
	public abstract class HttpClientServiceAspect
	{
		/// <summary>
		/// Called right before the request is sent.
		/// </summary>
		public Task RequestReadyAsync(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken)
		{
			return RequestReadyAsyncCore(httpRequest, requestDto, cancellationToken);
		}

		/// <summary>
		/// Called right after the response is received.
		/// </summary>
		public Task ResponseReceivedAsync(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
		{
			return ResponseReceivedAsyncCore(httpResponse, cancellationToken);
		}

		/// <summary>
		/// Creates an aspect.
		/// </summary>
		protected HttpClientServiceAspect()
		{
		}

		/// <summary>
		/// Called right before the request is sent.
		/// </summary>
		protected virtual Task RequestReadyAsyncCore(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken)
		{
			return Task.FromResult<object>(null);
		}

		/// <summary>
		/// Called right after the response is received.
		/// </summary>
		protected virtual Task ResponseReceivedAsyncCore(HttpResponseMessage httpResponse, CancellationToken cancellationToken)
		{
			return Task.FromResult<object>(null);
		}
	}
}
