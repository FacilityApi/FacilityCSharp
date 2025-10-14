using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text;

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
		m_enableRequestCompression = settings.CompressRequests ?? defaults.CompressRequests;
		m_synchronous = settings.Synchronous;
		m_skipRequestValidation = settings.SkipRequestValidation;
		m_skipResponseValidation = settings.SkipResponseValidation;
		m_disableChunkedTransfer = settings.DisableChunkedTransfer;

		var baseUri = settings.BaseUri ?? defaults.BaseUri;
		m_baseUrl = baseUri == null ? "/" : (baseUri.IsAbsoluteUri ? baseUri.AbsoluteUri : baseUri.OriginalString).TrimEnd('/') + "/";

		BaseUri = baseUri;
		ContentSerializer = settings.ContentSerializer ?? defaults.ContentSerializer ?? HttpContentSerializer.Legacy;
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
	/// The underlying <see cref="HttpClient"/>.
	/// </summary>
	protected HttpClient HttpClient => m_httpClient;

	private async Task<ServiceResult<(HttpRequestMessage HttpRequest, HttpResponseMessage HttpResponse, HttpResponseMapping<TResponse> ResponseMapping)>> TryStartRequestAsync<TRequest, TResponse>(HttpMethodMapping<TRequest, TResponse> mapping, TRequest request, CancellationToken cancellationToken)
		where TRequest : ServiceDto, new()
		where TResponse : ServiceDto, new()
	{
		HttpRequestMessage? httpRequest = null;
		HttpResponseMessage? httpResponse = null;
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

			// add Accept header if not JSON
			httpRequest = httpRequestResult.Value;
			var contentMediaType = ContentSerializer.DefaultMediaType;
			if (contentMediaType != HttpServiceUtility.JsonMediaType)
			{
				httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentMediaType));
				httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
			}

			// create the request body if necessary
			var requestBody = mapping.GetRequestBody(request);
			if (requestBody != null)
			{
				var contentType = mapping.RequestBodyContentType ?? requestHeaders?.GetContentType();
				httpRequest.Content = GetHttpContentSerializer(requestBody.GetType()).CreateHttpContent(requestBody, contentType);
				if (m_enableRequestCompression)
					httpRequest.Content = new CompressingHttpContent(httpRequest.Content);
				if (m_disableChunkedTransfer)
					await httpRequest.Content.LoadIntoBufferAsync(cancellationToken).ConfigureAwait(false);
			}

			// send the HTTP request and get the HTTP response
			httpResponse = await SendRequestAsync(httpRequest, request, cancellationToken).ConfigureAwait(false);

			// find the response mapping for the status code
			var statusCode = httpResponse.StatusCode;
			var responseMapping = mapping.ResponseMappings.FirstOrDefault(x => x.StatusCode == statusCode);

			// fail if no response mapping can be found for the status code
			if (responseMapping == null)
				return ServiceResult.Failure(await CreateErrorFromHttpResponseAsync(httpResponse, cancellationToken).ConfigureAwait(false));

			var returnValue = (httpRequest, httpResponse, responseMapping);
			httpRequest = null;
			httpResponse = null;
			return ServiceResult.Success(returnValue);
		}
		finally
		{
			httpResponse?.Dispose();
			httpRequest?.Dispose();
		}
	}

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

		HttpRequestMessage? httpRequest = null;
		HttpResponseMessage? httpResponse = null;
		try
		{
			var startRequestResult = await TryStartRequestAsync(mapping, request, cancellationToken).ConfigureAwait(false);
			if (startRequestResult.IsFailure)
				return startRequestResult.ToFailure();
			(httpRequest, httpResponse, var responseMapping) = startRequestResult.Value;

			// read the response body if necessary
			object? responseBody = null;
			if (responseMapping.ResponseBodyType != null)
			{
				var serializer = GetHttpContentSerializer(responseMapping.ResponseBodyType);
				var responseResult = await serializer.ReadHttpContentOrNullAsync(
					responseMapping.ResponseBodyType, httpResponse.Content, cancellationToken).ConfigureAwait(false);
				if (responseResult.IsFailure)
				{
					var error = responseResult.Error;
					error.Code = ServiceErrors.InvalidResponse;
					return ServiceResult.Failure(error);
				}
				responseBody = responseResult.Value;
			}

			// create the response DTO
			var response = responseMapping.CreateResponse(responseBody);
			response = mapping.SetResponseHeaders(response, HttpServiceUtility.CreateDictionaryFromHeaders(httpResponse.Headers, httpResponse.Content?.Headers)!);

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
		finally
		{
			httpResponse?.Dispose();
			httpRequest?.Dispose();
		}
	}

	/// <summary>
	/// Sends an HTTP request for an event and processes the response.
	/// </summary>
	protected async Task<ServiceResult<IAsyncEnumerable<ServiceResult<TResponse>>>> TrySendEventRequestAsync<TRequest, TResponse>(HttpMethodMapping<TRequest, TResponse> mapping, TRequest request, CancellationToken cancellationToken)
		where TRequest : ServiceDto, new()
		where TResponse : ServiceDto, new()
	{
		if (mapping == null)
			throw new ArgumentNullException(nameof(mapping));
		if (request == null)
			throw new ArgumentNullException(nameof(request));

		HttpRequestMessage? httpRequest = null;
		HttpResponseMessage? httpResponse = null;
		try
		{
			var startRequestResult = await TryStartRequestAsync(mapping, request, cancellationToken).ConfigureAwait(false);
			if (startRequestResult.IsFailure)
				return startRequestResult.ToFailure();
			(httpRequest, httpResponse, var responseMapping) = startRequestResult.Value;

			var enumerable = CreateAsyncEnumerable(httpRequest, httpResponse, responseMapping, ContentSerializer, m_skipResponseValidation, cancellationToken);
			httpResponse = null;
			httpRequest = null;

			return ServiceResult.Success(enumerable);
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
		finally
		{
			httpResponse?.Dispose();
			httpRequest?.Dispose();
		}
	}

	private async IAsyncEnumerable<ServiceResult<TResponse>> CreateAsyncEnumerable<TResponse>(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse, HttpResponseMapping<TResponse> responseMapping, HttpContentSerializer contentSerializer, bool skipResponseValidation, [EnumeratorCancellation] CancellationToken cancellationToken)
		where TResponse : ServiceDto, new()
	{
		Stream? stream = null;
		try
		{
#if !NETSTANDARD2_0
			stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
			stream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif

			var sseParser = SseParser.Create(stream);
			var enumerator = sseParser.EnumerateAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
			await using var enumeratorScope = enumerator.ConfigureAwait(false);
			while (true)
			{
				ServiceResult<TResponse> nextResult;
				var stopStream = false;

				try
				{
					if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
						break;

					var sseItem = enumerator.Current;
					var isError = sseItem.EventType == "error";
					if (!isError && sseItem.EventType != SseParser.EventTypeDefault)
					{
						nextResult = ServiceResult.Failure(ServiceErrors.CreateInternalError("Unexpected event type: " + sseItem.EventType));
						stopStream = true;
					}
					else
					{
						var type = isError ? typeof(ServiceErrorDto) : responseMapping.ResponseBodyType!;
						using var content = new StringContent(sseItem.Data, Encoding.UTF8, HttpServiceUtility.JsonMediaType);
						var responseResult = await contentSerializer.ReadHttpContentAsync(type, content, cancellationToken).ConfigureAwait(false);
						if (responseResult.IsFailure)
						{
							var error = responseResult.Error;
							error.Code = ServiceErrors.InvalidResponse;
							nextResult = ServiceResult.Failure(error);
							stopStream = true;
						}
						else if (isError)
						{
							nextResult = ServiceResult.Failure((ServiceErrorDto) responseResult.Value);
						}
						else
						{
							var response = responseMapping.CreateResponse(responseResult.Value);
							if (!skipResponseValidation && !response.Validate(out var responseErrorMessage))
							{
								nextResult = ServiceResult.Failure(ServiceErrors.CreateInvalidResponse(responseErrorMessage));
								stopStream = true;
							}
							else
							{
								nextResult = ServiceResult.Success((TResponse) responseResult.Value);
							}
						}
					}
				}
				catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
				{
					// HttpClient timeout
					nextResult = ServiceResult.Failure(ServiceErrors.CreateTimeout());
					stopStream = true;
				}
				catch (Exception exception) when (ShouldCreateErrorFromException(exception))
				{
					// cancellation can cause the wrong exception
					cancellationToken.ThrowIfCancellationRequested();

					// error contacting service
					nextResult = ServiceResult.Failure(CreateErrorFromException(exception));
					stopStream = true;
				}

				yield return nextResult;

				if (stopStream)
					break;
			}
		}
		finally
		{
#if !NETSTANDARD2_0
			if (stream is not null)
				await stream.DisposeAsync().ConfigureAwait(false);
#else
			stream?.Dispose();
#endif
			httpResponse.Dispose();
			httpRequest.Dispose();
		}
	}

	/// <summary>
	/// Called to create an error object from an unhandled HTTP response.
	/// </summary>
	protected virtual async Task<ServiceErrorDto> CreateErrorFromHttpResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
	{
		var result = await ContentSerializer.ReadHttpContentOrNullAsync<ServiceErrorDto>(response.Content, cancellationToken).ConfigureAwait(false);

		if (result.IsFailure || result.Value is null || string.IsNullOrWhiteSpace(result.Value.Code))
			return HttpServiceErrors.CreateErrorForStatusCode(response.StatusCode, response.ReasonPhrase);

		return result.Value;
	}

	/// <summary>
	/// Called to determine if an error object should be created from an unexpected exception.
	/// </summary>
	protected virtual bool ShouldCreateErrorFromException(Exception exception)
	{
		if (exception is ArgumentException or ObjectDisposedException or AggregateException or UriFormatException)
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

	/// <summary>
	/// Called right before the request is sent, before aspects are applied.
	/// </summary>
	protected virtual Task RequestReadyAsync(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken) => Task.CompletedTask;

	protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken)
	{
		await AdaptTask(RequestReadyAsync(httpRequest, requestDto, cancellationToken)).ConfigureAwait(true);

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
#if !NETSTANDARD2_0
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

	private sealed class CompressingHttpContent : HttpContent
	{
		public CompressingHttpContent(HttpContent baseContent)
		{
			m_baseContent = baseContent;
			foreach (var header in baseContent.Headers)
			{
				// remove Content-Length header, if present, as it will change
				if (!header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
					Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			Headers.ContentEncoding.Clear();
			Headers.ContentEncoding.Add("gzip");
		}

		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
			DoSerializeToStreamAsync(stream, CancellationToken.None);

#if NET8_0_OR_GREATER
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken) =>
			DoSerializeToStreamAsync(stream, cancellationToken);
#endif

		private async Task DoSerializeToStreamAsync(Stream stream, CancellationToken cancellationToken)
		{
#if NET5_0_OR_GREATER
			using var baseContentStream = await m_baseContent.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
			using var baseContentStream = await m_baseContent.ReadAsStreamAsync().ConfigureAwait(false);
#endif
			using var gzipStream = new GZipStream(stream, CompressionLevel.Optimal, leaveOpen: true);
			using var bufferedStream = new BufferedStream(gzipStream, 8192);
#if NETCOREAPP2_1_OR_GREATER
			await baseContentStream.CopyToAsync(bufferedStream, cancellationToken).ConfigureAwait(false);
#else
			await baseContentStream.CopyToAsync(bufferedStream, 8192, cancellationToken).ConfigureAwait(false);
#endif
		}

		protected override bool TryComputeLength(out long length)
		{
			length = -1L;
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					m_baseContent.Dispose();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		private readonly HttpContent m_baseContent;
	}

	private static readonly HttpClient s_defaultHttpClient = HttpServiceUtility.CreateHttpClient();

	private readonly HttpClient m_httpClient;
	private readonly IReadOnlyList<HttpClientServiceAspect>? m_aspects;
	private readonly bool m_enableRequestCompression;
	private readonly bool m_synchronous;
	private readonly bool m_skipRequestValidation;
	private readonly bool m_skipResponseValidation;
	private readonly bool m_disableChunkedTransfer;
	private readonly string m_baseUrl;
}
