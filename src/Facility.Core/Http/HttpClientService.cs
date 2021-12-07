using System.Diagnostics.CodeAnalysis;

namespace Facility.Core.Http;

/// <summary>
/// Used by HTTP clients.
/// </summary>
public abstract class HttpClientService
{
	/// <summary>
	/// Creates an instance with the specified settings and defaults.
	/// </summary>
	protected HttpClientService(HttpClientServiceSettings? settings, HttpClientServiceDefaults defaults)
	{
		settings ??= new HttpClientServiceSettings();

		m_httpClient = settings.HttpClient ?? s_defaultHttpClient;
		m_aspects = settings.Aspects;
		m_synchronous = settings.Synchronous;
		m_skipRequestValidation = settings.SkipRequestValidation;
		m_skipResponseValidation = settings.SkipResponseValidation;

		var baseUri = settings.BaseUri ?? defaults.BaseUri;
		m_baseUrl = baseUri == null ? "/" : (baseUri.IsAbsoluteUri ? baseUri.AbsoluteUri : baseUri.OriginalString).TrimEnd('/') + "/";

		BaseUri = baseUri;
		ContentSerializer = settings.ContentSerializer ?? new JsonHttpContentSerializer(settings.JsonSerializer ?? defaults.JsonSerializer ?? JsonServiceSerializer.Legacy);
		BytesSerializer = settings.BytesSerializer ?? BytesHttpContentSerializer.Instance;
		TextSerializer = settings.TextSerializer ?? TextHttpContentSerializer.Instance;
	}

	/// <summary>
	/// Creates an instance with the specified settings.
	/// </summary>
	[Obsolete("Regenerate code to use the new constructor.")]
	protected HttpClientService(HttpClientServiceSettings? settings, Uri? defaultBaseUri)
		: this(settings, new HttpClientServiceDefaults { BaseUri = defaultBaseUri })
	{
	}

	/// <summary>
	/// The HTTP content serializer.
	/// </summary>
	protected HttpContentSerializer ContentSerializer { get; }

	/// <summary>
	/// The HTTP content serializer for bytes.
	/// </summary>
	protected HttpContentSerializer BytesSerializer { get; }

	/// <summary>
	/// The HTTP content serializer for text.
	/// </summary>
	protected HttpContentSerializer TextSerializer { get; }

	/// <summary>
	/// The base URI.
	/// </summary>
	protected Uri? BaseUri { get; }

	/// <summary>
	/// Sends an HTTP request and processes the response.
	/// </summary>
	protected async Task<ServiceResult<TResponse>> TrySendRequestAsync<TRequest, TResponse>(HttpMethodMapping<TRequest, TResponse> mapping, TRequest request, CancellationToken cancellationToken)
		where TRequest : ServiceDto, new()
		where TResponse : ServiceDto, new()
	{
		if (mapping == null)
			throw new ArgumentNullException(nameof(mapping));
		if (request == null)
			throw new ArgumentNullException(nameof(request));

		try
		{
			// validate the request DTO
			if (!m_skipRequestValidation && !request.Validate(out var requestErrorMessage))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidRequest(requestErrorMessage));

			// make sure the request DTO doesn't violate any HTTP constraints
			var requestValidation = mapping.ValidateRequest(request);
			if (requestValidation.IsFailure)
				return requestValidation.ToFailure();

			// create the HTTP request with the right method, path, query string, and headers
			var requestHeaders = mapping.GetRequestHeaders(request);
			var httpRequestResult = TryCreateHttpRequest(mapping.HttpMethod, mapping.Path, mapping.GetUriParameters(request), requestHeaders);
			if (httpRequestResult.IsFailure)
				return httpRequestResult.ToFailure();

			// create the request body if necessary
			using var httpRequest = httpRequestResult.Value;
			var requestBody = mapping.GetRequestBody(request);
			if (requestBody != null)
			{
				var contentType = mapping.RequestBodyContentType ?? requestHeaders?.GetContentType();
				httpRequest.Content = GetHttpContentSerializer(requestBody.GetType()).CreateHttpContent(requestBody, contentType);
			}

			// send the HTTP request and get the HTTP response
			using var httpResponse = await SendRequestAsync(httpRequest, request, cancellationToken).ConfigureAwait(false);

			// find the response mapping for the status code
			var statusCode = httpResponse.StatusCode;
			var responseMapping = mapping.ResponseMappings.FirstOrDefault(x => x.StatusCode == statusCode);

			// fail if no response mapping can be found for the status code
			if (responseMapping == null)
				return ServiceResult.Failure(await CreateErrorFromHttpResponseAsync(httpResponse, cancellationToken).ConfigureAwait(false));

			// read the response body if necessary
			object? responseBody = null;
			if (responseMapping.ResponseBodyType != null)
			{
				var serializer = GetHttpContentSerializer(responseMapping.ResponseBodyType);
				var responseResult = await serializer.ReadHttpContentAsync(
					responseMapping.ResponseBodyType, httpResponse.Content, cancellationToken).ConfigureAwait(false);
				if (responseResult.IsFailure)
				{
					var error = responseResult.Error!;
					error.Code = ServiceErrors.InvalidResponse;
					return ServiceResult.Failure(error);
				}
				responseBody = responseResult.Value;
			}

			// create the response DTO
			var response = responseMapping.CreateResponse(responseBody);
			response = mapping.SetResponseHeaders(response, HttpServiceUtility.CreateDictionaryFromHeaders(httpResponse.Headers, httpResponse.Content.Headers)!);

			// validate the response DTO
			if (!m_skipResponseValidation && !response.Validate(out var responseErrorMessage))
				return ServiceResult.Failure(ServiceErrors.CreateInvalidResponse(responseErrorMessage));

			return ServiceResult.Success(response);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			// HttpClient timeout
			return ServiceResult.Failure(ServiceErrors.CreateTimeout());
		}
		catch (Exception exception) when (ShouldCreateErrorFromException(exception))
		{
			// cancellation can cause the wrong exception
			cancellationToken.ThrowIfCancellationRequested();

			// error contacting service
			return ServiceResult.Failure(CreateErrorFromException(exception));
		}
	}

	/// <summary>
	/// Called to create an error object from an unhandled HTTP response.
	/// </summary>
	protected virtual async Task<ServiceErrorDto> CreateErrorFromHttpResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
	{
		var result = await ContentSerializer.ReadHttpContentAsync<ServiceErrorDto>(response.Content, cancellationToken).ConfigureAwait(false);

		if (result.IsFailure || string.IsNullOrWhiteSpace(result.Value.Code))
			return HttpServiceErrors.CreateErrorForStatusCode(response.StatusCode, response.ReasonPhrase);

		return result.Value;
	}

	/// <summary>
	/// Called to determine if an error object should be created from an unexpected exception.
	/// </summary>
	protected virtual bool ShouldCreateErrorFromException(Exception exception)
	{
		if (exception is ArgumentException || exception is ObjectDisposedException || exception is AggregateException)
			return true;

		var exceptionTypeName = exception.GetType().FullName;
		return exceptionTypeName != null && (
			exceptionTypeName.StartsWith("System.Net.", StringComparison.Ordinal) ||
			exceptionTypeName.StartsWith("System.IO.", StringComparison.Ordinal) ||
			exceptionTypeName.StartsWith("System.Web.", StringComparison.Ordinal));
	}

	/// <summary>
	/// Called to create an error object from an unexpected exception.
	/// </summary>
	protected virtual ServiceErrorDto CreateErrorFromException(Exception exception) => ServiceErrorUtility.CreateInternalErrorForException(exception);

	private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken)
	{
		if (m_aspects != null)
		{
			foreach (var aspect in m_aspects)
				await AdaptTask(aspect.RequestReadyAsync(httpRequest, requestDto, cancellationToken)).ConfigureAwait(true);
		}

		var httpResponse = await AdaptTask(m_httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)).ConfigureAwait(true);

		if (m_aspects != null)
		{
			foreach (var aspect in m_aspects)
				await AdaptTask(aspect.ResponseReceivedAsync(httpResponse, requestDto, cancellationToken)).ConfigureAwait(true);
		}

		return httpResponse;
	}

	private ServiceResult<HttpRequestMessage> TryCreateHttpRequest(HttpMethod httpMethod, string relativeUrlPattern, IEnumerable<KeyValuePair<string, string?>>? uriParameters, IEnumerable<KeyValuePair<string, string?>>? requestHeaders)
	{
		var url = m_baseUrl + relativeUrlPattern.TrimStart('/');
		if (uriParameters != null)
			url = GetUrlFromPattern(url, uriParameters);

		var requestMessage = new HttpRequestMessage(httpMethod, url);

		var headersResult = HttpServiceUtility.TryAddNonContentHeaders(requestMessage.Headers, requestHeaders);
		if (headersResult.IsFailure)
			return headersResult.ToFailure();

		return ServiceResult.Success(requestMessage);
	}

	private static string GetUrlFromPattern(string url, IEnumerable<KeyValuePair<string, string?>> parameters)
	{
		var hasQuery = url.IndexOfOrdinal('?') != -1;

		foreach (var parameter in parameters)
		{
			if (parameter.Key != null && parameter.Value != null)
			{
				var bracketedKey = "{" + parameter.Key + "}";
				var bracketedKeyIndex = url.IndexOf(bracketedKey, StringComparison.Ordinal);
				if (bracketedKeyIndex != -1)
				{
#if NET6_0_OR_GREATER
					url = string.Concat(url.AsSpan(0, bracketedKeyIndex), Uri.EscapeDataString(parameter.Value), url.AsSpan(bracketedKeyIndex + bracketedKey.Length));
#else
						url = url.Substring(0, bracketedKeyIndex) + Uri.EscapeDataString(parameter.Value) + url.Substring(bracketedKeyIndex + bracketedKey.Length);
#endif
				}
				else
				{
					url += (hasQuery ? "&" : "?") + Uri.EscapeDataString(parameter.Key) + "=" + Uri.EscapeDataString(parameter.Value);
					hasQuery = true;
				}
			}
		}

		return url;
	}

	[SuppressMessage("Performance", "CA1849:Call async methods when in an async method", Justification = "Task is completed.")]
	private Task AdaptTask(Task task)
	{
		if (!m_synchronous)
			return task;

#pragma warning disable CA1849
		task.GetAwaiter().GetResult();
#pragma warning restore CA1849
		return Task.CompletedTask;
	}

	private Task<T> AdaptTask<T>(Task<T> task)
	{
		if (!m_synchronous)
			return task;

		return Task.FromResult(task.GetAwaiter().GetResult());
	}

	private HttpContentSerializer GetHttpContentSerializer(Type objectType) =>
		HttpServiceUtility.UsesBytesSerializer(objectType) ? BytesSerializer :
		HttpServiceUtility.UsesTextSerializer(objectType) ? TextSerializer :
		ContentSerializer;

	private static readonly HttpClient s_defaultHttpClient = HttpServiceUtility.CreateHttpClient();

	private readonly HttpClient m_httpClient;
	private readonly IReadOnlyList<HttpClientServiceAspect>? m_aspects;
	private readonly bool m_synchronous;
	private readonly bool m_skipRequestValidation;
	private readonly bool m_skipResponseValidation;
	private readonly string m_baseUrl;
}
