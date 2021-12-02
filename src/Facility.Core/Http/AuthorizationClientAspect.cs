using System.Net.Http.Headers;

namespace Facility.Core.Http;

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
	protected override Task RequestReadyAsyncCore(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken)
	{
		if (m_authorizationHeader != null)
			httpRequest.Headers.Authorization = m_authorizationHeader;
		return Task.CompletedTask;
	}

	private AuthorizationClientAspect(string authorizationHeader)
	{
		if (!string.IsNullOrWhiteSpace(authorizationHeader))
			m_authorizationHeader = AuthenticationHeaderValue.Parse(authorizationHeader);
	}

	private readonly AuthenticationHeaderValue? m_authorizationHeader;
}
