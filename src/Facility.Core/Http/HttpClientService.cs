using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core.Http
{
	/// <summary>
	/// Used by HTTP clients.
	/// </summary>
	public sealed class HttpClientService
	{
		/// <summary>
		/// Creates an instance with the specified settings.
		/// </summary>
		public HttpClientService(HttpClientServiceSettings settings)
			: this(settings, defaultBaseUri: null)
		{
		}

		/// <summary>
		/// Creates an instance with the specified settings.
		/// </summary>
		public HttpClientService(HttpClientServiceSettings settings, Uri defaultBaseUri)
		{
			settings = settings ?? new HttpClientServiceSettings();

			m_httpClient = settings.HttpClient ?? s_defaultHttpClient;
			m_baseUri = settings.BaseUri ?? m_httpClient.BaseAddress ?? defaultBaseUri;
			m_contentSerializer = settings.ContentSerializer;
			m_aspects = settings.Aspects;
			m_synchronous = settings.Synchronous;

			if (m_baseUri == null || !m_baseUri.IsAbsoluteUri)
				throw new ArgumentException("BaseUri (or HttpClient.BaseAddress) must be specified and absolute.");

			if (m_contentSerializer == null)
				m_contentSerializer = JsonHttpContentSerializer.Instance;
		}

		/// <summary>
		/// Sends an HTTP request and processes the response.
		/// </summary>
		public async Task<ServiceResult<TResponse>> TrySendRequestAsync<TRequest, TResponse>(HttpMethodMapping<TRequest, TResponse> mapping, TRequest request, CancellationToken cancellationToken)
			where TRequest : ServiceDto, new()
			where TResponse : ServiceDto, new()
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			try
			{
				// validate the request DTO
				var requestValidation = mapping.ValidateRequest(request);
				if (requestValidation.IsFailure)
					return requestValidation.AsFailure();

				// create the HTTP request with the right method, path, query string, and headers
				var httpRequestResult = TryCreateHttpRequest(mapping.HttpMethod, mapping.Path, mapping.GetUriParameters(request), mapping.GetRequestHeaders(request));
				if (httpRequestResult.IsFailure)
					return httpRequestResult.AsFailure();
				var httpRequest = httpRequestResult.Value;

				// create the request body if necessary
				var requestBody = mapping.GetRequestBody(request);
				if (requestBody != null)
					httpRequest.Content = m_contentSerializer.CreateHttpContent(requestBody);

				// send the HTTP request and get the HTTP response
				var httpResponse = await SendRequestAsync(httpRequest, request, cancellationToken).ConfigureAwait(false);

				// find the response mapping for the status code
				var statusCode = httpResponse.StatusCode;
				var responseMapping = mapping.ResponseMappings.FirstOrDefault(x => x.StatusCode == statusCode);

				// adding the first field by changing 204 to 200 is not a breaking change
				if (responseMapping == null && statusCode == HttpStatusCode.OK)
					responseMapping = mapping.ResponseMappings.FirstOrDefault(x => x.StatusCode == HttpStatusCode.NoContent);

				// fail if no response mapping can be found for the status code
				if (responseMapping == null)
					return ServiceResult.Failure(await CreateErrorFromHttpResponseAsync(httpResponse).ConfigureAwait(false));

				// read the response body if necessary
				ServiceDto responseBody = null;
				if (responseMapping.ResponseBodyType != null)
				{
					ServiceResult<ServiceDto> responseResult = await m_contentSerializer.ReadHttpContentAsync(
						responseMapping.ResponseBodyType, httpResponse.Content).ConfigureAwait(false);
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
				response = mapping.SetResponseHeaders(response, HttpServiceUtility.CreateDictionaryFromHeaders(httpResponse.Headers));
				return ServiceResult.Success(response);
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				// HttpClient timeout
				return ServiceResult.Failure(ServiceErrors.CreateTimeout());
			}
			catch (Exception exception) when (exception is ArgumentException || exception is HttpRequestException || exception is IOException)
			{
				// cancellation can cause the wrong exception
				cancellationToken.ThrowIfCancellationRequested();

				// error contacting service
				return ServiceResult.Failure(ServiceErrorUtility.CreateInternalErrorForException(exception));
			}
		}

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

		private ServiceResult<HttpRequestMessage> TryCreateHttpRequest(HttpMethod httpMethod, string relativeUriPattern, IEnumerable<KeyValuePair<string, string>> uriParameters, IEnumerable<KeyValuePair<string, string>> requestHeaders)
		{
			string uriText = m_baseUri.AbsoluteUri;

			if (!string.IsNullOrEmpty(relativeUriPattern))
				uriText = uriText.TrimEnd('/') + "/" + relativeUriPattern.TrimStart('/');

			Uri uri = uriParameters != null ? GetUriFromPattern(uriText, uriParameters) : new Uri(uriText);
			var requestMessage = new HttpRequestMessage(httpMethod, uri);

			string defaultMediaType = m_contentSerializer.DefaultMediaType;
			if (!string.IsNullOrEmpty(defaultMediaType))
				requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(defaultMediaType));

			var headersResult = HttpServiceUtility.TryAddHeaders(requestMessage.Headers, requestHeaders);
			if (headersResult.IsFailure)
				return headersResult.AsFailure();

			return ServiceResult.Success(requestMessage);
		}

		private static Uri GetUriFromPattern(string uriPattern, IEnumerable<KeyValuePair<string, string>> parameters)
		{
			bool hasQuery = uriPattern.IndexOf('?') != -1;

			foreach (KeyValuePair<string, string> parameter in parameters)
			{
				if (parameter.Key != null && parameter.Value != null)
				{
					string bracketedKey = "{" + parameter.Key + "}";
					int bracketedKeyIndex = uriPattern.IndexOf(bracketedKey, StringComparison.Ordinal);
					if (bracketedKeyIndex != -1)
					{
						uriPattern = uriPattern.Substring(0, bracketedKeyIndex) +
							Uri.EscapeDataString(parameter.Value) + uriPattern.Substring(bracketedKeyIndex + bracketedKey.Length);
					}
					else
					{
						uriPattern += (hasQuery ? "&" : "?") + Uri.EscapeDataString(parameter.Key) + "=" + Uri.EscapeDataString(parameter.Value);
						hasQuery = true;
					}
				}
			}

			return new Uri(uriPattern);
		}

		private async Task<ServiceErrorDto> CreateErrorFromHttpResponseAsync(HttpResponseMessage response)
		{
			var result = await m_contentSerializer.ReadHttpContentAsync<ServiceErrorDto>(response.Content).ConfigureAwait(false);

			if (result.IsFailure || string.IsNullOrWhiteSpace(result.Value?.Code))
				return HttpServiceErrors.CreateErrorForStatusCode(response.StatusCode, response.ReasonPhrase);

			return result.Value;
		}

		private Task AdaptTask(Task task)
		{
			if (!m_synchronous)
				return task;

			task.GetAwaiter().GetResult();
			return Task.FromResult<object>(null);
		}

		private Task<T> AdaptTask<T>(Task<T> task)
		{
			if (!m_synchronous)
				return task;

			return Task.FromResult(task.GetAwaiter().GetResult());
		}

		static readonly HttpClient s_defaultHttpClient = HttpServiceUtility.CreateHttpClient();

		readonly bool m_synchronous;
		readonly Uri m_baseUri;
		readonly HttpClient m_httpClient;
		readonly HttpContentSerializer m_contentSerializer;
		readonly IReadOnlyList<HttpClientServiceAspect> m_aspects;
	}
}
