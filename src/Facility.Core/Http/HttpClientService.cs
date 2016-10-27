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
			m_aspects = settings.Aspects;
			m_synchronous = settings.Synchronous;
			m_mediaType = settings.MediaType ?? HttpServiceUtility.JsonMediaType;

			if (m_baseUri == null || !m_baseUri.IsAbsoluteUri)
				throw new ArgumentException("BaseUri (or HttpClient.BaseAddress) must be specified and absolute.");
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

				// create the HTTP request with the right method, path, and query string
				var httpRequestResult = TryCreateRequest(mapping.HttpMethod, mapping.Path, mapping.GetUriParameters(request), mapping.GetRequestHeaders(request));
				if (httpRequestResult.IsFailure)
					return httpRequestResult.AsFailure();
				var httpRequest = httpRequestResult.Value;

				// create the request body if necessary
				var requestBody = mapping.GetRequestBody(request);
				if (requestBody != null)
					httpRequest.Content = await CreateHttpContentAsync(requestBody).ConfigureAwait(false);

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
					return ServiceResult.Failure(await HttpServiceUtility.CreateErrorFromHttpResponseAsync(httpResponse).ConfigureAwait(false));

				// read the response body if necessary
				ServiceDto responseBody = null;
				if (responseMapping.ResponseBodyType != null)
				{
					ServiceResult<ServiceDto> responseResult = await HttpServiceUtility.TryReadHttpContentAsync(
						responseMapping.ResponseBodyType, httpResponse.Content, ServiceErrors.InvalidResponse).ConfigureAwait(false);
					if (responseResult.IsFailure)
						return responseResult.AsFailure();
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

		/// <summary>
		/// Sends an HTTP request.
		/// </summary>
		public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage httpRequest, ServiceDto requestDto, CancellationToken cancellationToken)
		{
			if (httpRequest == null)
				throw new ArgumentNullException(nameof(httpRequest));

			httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(m_mediaType));

			if (m_aspects != null)
			{
				foreach (var aspect in m_aspects)
					await AdaptTask(aspect.RequestReadyAsync(httpRequest, requestDto, cancellationToken)).ConfigureAwait(true);
			}

			var httpResponse = await AdaptTask(m_httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)).ConfigureAwait(true);

			if (m_aspects != null)
			{
				foreach (var aspect in m_aspects)
					await AdaptTask(aspect.ResponseReceivedAsync(httpResponse, cancellationToken)).ConfigureAwait(true);
			}

			return httpResponse;
		}

		/// <summary>
		/// Creates an HTTP request.
		/// </summary>
		public HttpRequestMessage CreateRequest(HttpMethod httpMethod, string relativeUriPattern, IEnumerable<KeyValuePair<string, string>> uriParameters = null)
		{
			return TryCreateRequest(httpMethod, relativeUriPattern, uriParameters).GetValueOrDefault();
		}

		/// <summary>
		/// Creates an HTTP request.
		/// </summary>
		public ServiceResult<HttpRequestMessage> TryCreateRequest(HttpMethod httpMethod, string relativeUriPattern, IEnumerable<KeyValuePair<string, string>> uriParameters = null, IEnumerable<KeyValuePair<string, string>> requestHeaders = null)
		{
			return HttpServiceUtility.TryCreateHttpRequest(m_baseUri, httpMethod, relativeUriPattern, uriParameters, requestHeaders);
		}

		/// <summary>
		/// Creates HTTP content.
		/// </summary>
		public Task<HttpContent> CreateHttpContentAsync(ServiceDto content)
		{
			return HttpServiceUtility.CreateHttpContentAsync(content, m_mediaType);
		}

		/// <summary>
		/// Creates a default HTTP client.
		/// </summary>
		public static HttpClient CreateDefaultHttpClient()
		{
			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.ExpectContinue = false;
			return httpClient;
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

		static readonly HttpClient s_defaultHttpClient = CreateDefaultHttpClient();

		readonly bool m_synchronous;
		readonly Uri m_baseUri;
		readonly HttpClient m_httpClient;
		readonly string m_mediaType;
		readonly IReadOnlyList<HttpClientServiceAspect> m_aspects;
	}
}
