namespace Facility.Core.Http;

/// <summary>
/// The context for service HTTP handlers.
/// </summary>
public sealed class ServiceHttpContext
{
	/// <summary>
	/// The current service request.
	/// </summary>
	public ServiceDto? Request { get; internal set; }

	/// <summary>
	/// The current service result.
	/// </summary>
	public ServiceResult<ServiceDto>? Result { get; internal set; }

	/// <summary>
	/// Attempts to get the context from the specified HTTP request.
	/// </summary>
	public static ServiceHttpContext? TryGetContext(HttpRequestMessage httpRequest)
	{
#if !NETSTANDARD2_0
		httpRequest.Options.TryGetValue(s_requestPropertyContextKey, out var context);
		return context;
#else
		httpRequest.Properties.TryGetValue(c_requestPropertyContextKey, out var context);
		return context as ServiceHttpContext;
#endif
	}

	/// <summary>
	/// Attempts to get the context from the specified HTTP response.
	/// </summary>
	public static ServiceHttpContext? TryGetContext(HttpResponseMessage httpResponse)
	{
		var httpRequest = httpResponse.RequestMessage;
		return httpRequest != null ? TryGetContext(httpRequest) : null;
	}

	internal static void SetContext(HttpRequestMessage httpRequest, ServiceHttpContext context)
	{
#if !NETSTANDARD2_0
		httpRequest.Options.Set(s_requestPropertyContextKey, context);
#else
		httpRequest.Properties[c_requestPropertyContextKey] = context;
#endif
	}

#if !NETSTANDARD2_0
	private static readonly HttpRequestOptionsKey<ServiceHttpContext> s_requestPropertyContextKey = new("Facility_Context");
#else
	private const string c_requestPropertyContextKey = "Facility_Context";
#endif
}
