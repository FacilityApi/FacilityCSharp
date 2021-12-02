namespace Facility.Core.Http;

/// <summary>
/// Common implementations of <see cref="HttpClientServiceAspect"/>.
/// </summary>
public static class CommonClientAspects
{
	/// <summary>
	/// Sets the Accept header of the request.
	/// </summary>
	public static HttpClientServiceAspect RequestAccept(string? accept) => RequestHeader("Accept", accept);

	/// <summary>
	/// Sets the Authorization header of the request.
	/// </summary>
	public static HttpClientServiceAspect RequestAuthorization(string? authorization) => RequestHeader("Authorization", authorization);

	/// <summary>
	/// Sets a request header.
	/// </summary>
	public static HttpClientServiceAspect RequestHeader(string headerName, string? headerValue)
	{
		if (string.IsNullOrWhiteSpace(headerName))
			throw new ArgumentException("Invalid header name.", nameof(headerName));

		return RequestReady(request =>
		{
			if (!string.IsNullOrEmpty(headerValue))
				request.Headers.Add(headerName, headerValue);
		});
	}

	/// <summary>
	/// Executes the action when the request is ready.
	/// </summary>
	public static HttpClientServiceAspect RequestReady(Action<HttpRequestMessage> action) => new RequestReadyClientAspect(action);

	/// <summary>
	/// Sets the User-Agent header of the request.
	/// </summary>
	public static HttpClientServiceAspect RequestUserAgent(string? userAgent) => RequestHeader("User-Agent", userAgent);

	private sealed class RequestReadyClientAspect : HttpClientServiceAspect
	{
		public RequestReadyClientAspect(Action<HttpRequestMessage> action)
		{
			m_action = action ?? throw new ArgumentNullException(nameof(action));
		}

		protected override Task RequestReadyAsyncCore(HttpRequestMessage request, ServiceDto requestDto, CancellationToken cancellationToken)
		{
			m_action(request);
			return Task.CompletedTask;
		}

		private readonly Action<HttpRequestMessage> m_action;
	}
}
