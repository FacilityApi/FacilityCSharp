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
		public Task RequestReadyAsync(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken) =>
			RequestReadyAsyncCore(httpRequest, requestDto, cancellationToken);

		/// <summary>
		/// Called right after the response is received.
		/// </summary>
		public Task ResponseReceivedAsync(HttpResponseMessage httpResponse, ServiceDto requestDto, CancellationToken cancellationToken) =>
			ResponseReceivedAsyncCore(httpResponse, requestDto, cancellationToken);

		/// <summary>
		/// Creates an aspect.
		/// </summary>
		protected HttpClientServiceAspect()
		{
		}

		/// <summary>
		/// Called right before the request is sent.
		/// </summary>
		protected virtual Task RequestReadyAsyncCore(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken) => Task.CompletedTask;

		/// <summary>
		/// Called right after the response is received.
		/// </summary>
		protected virtual Task ResponseReceivedAsyncCore(HttpResponseMessage httpResponse, ServiceDto requestDto, CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
