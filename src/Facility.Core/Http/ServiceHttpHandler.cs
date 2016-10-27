using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Facility.Core.Http
{
	/// <summary>
	/// A service HTTP handler.
	/// </summary>
	public abstract class ServiceHttpHandler : DelegatingHandler
	{
		/// <summary>
		/// Attempts to handle the HTTP request.
		/// </summary>
		public abstract Task<HttpResponseMessage> TryHandleHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken);

		/// <summary>
		/// Creates an instance.
		/// </summary>
		protected ServiceHttpHandler(ServiceHttpHandlerSettings settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			RootPath = (settings.RootPath ?? "").TrimEnd('/');
			IncludeErrorDetail = settings.IncludeErrorDetail;
			m_synchronous = settings.Synchronous;
			m_defaultMediaType = settings.DefaultMediaType ?? HttpServiceUtility.JsonMediaType;
			m_aspects = settings.Aspects;
		}

		/// <summary>
		/// The root path of the service.
		/// </summary>
		protected string RootPath { get; }

		/// <summary>
		/// True if potentially insecure error detail should be included for debugging purposes.
		/// </summary>
		protected bool IncludeErrorDetail { get; }

		/// <summary>
		/// Makes a task synchronous if necessary.
		/// </summary>
		protected Task AdaptTask(Task task)
		{
			if (!m_synchronous)
				return task;

			task.GetAwaiter().GetResult();
			return Task.FromResult<object>(null);
		}

		/// <summary>
		/// Makes a task synchronous if necessary.
		/// </summary>
		protected Task<T> AdaptTask<T>(Task<T> task)
		{
			if (!m_synchronous)
				return task;

			return Task.FromResult(task.GetAwaiter().GetResult());
		}

		/// <summary>
		/// Determines the best media type for the response.
		/// </summary>
		/// <param name="httpRequest"></param>
		/// <returns></returns>
		protected string GetResponseMediaType(HttpRequestMessage httpRequest)
		{
			return httpRequest.Headers.Accept
				.OrderByDescending(x => x.Quality)
				.Select(x => x.MediaType)
				.FirstOrDefault(HttpServiceUtility.IsSupportedMediaType) ?? m_defaultMediaType;
		}

		/// <summary>
		/// Called when the request is received.
		/// </summary>
		protected async Task<HttpResponseMessage> RequestReceivedAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
		{
			if (m_aspects != null)
			{
				foreach (var aspect in m_aspects)
				{
					var httpResponse = await aspect.RequestReceivedAsync(httpRequest, cancellationToken).ConfigureAwait(false);
					if (httpResponse != null)
						return httpResponse;
				}
			}

			return null;
		}

		/// <summary>
		/// Called right before the response is sent.
		/// </summary>
		protected async Task ResponseReadyAsync(HttpResponseMessage httpResponse, ServiceResult<ServiceDto> result, CancellationToken cancellationToken)
		{
			if (m_aspects != null)
			{
				foreach (var aspect in m_aspects)
					await aspect.ResponseReadyAsync(httpResponse, result, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Attempts to handle a service method.
		/// </summary>
		protected async Task<HttpResponseMessage> TryHandleServiceMethodAsync<TRequest, TResponse>(HttpMethodMapping<TRequest, TResponse> mapping, HttpRequestMessage httpRequest, Func<TRequest, CancellationToken, Task<ServiceResult<TResponse>>> invokeMethodAsync, CancellationToken cancellationToken)
			where TRequest : ServiceDto, new()
			where TResponse : ServiceDto, new()
		{
			if (httpRequest.Method != mapping.HttpMethod)
				return null;

			var pathParameters = HttpServiceUtility.TryMatchHttpRoute(httpRequest.RequestUri, RootPath + mapping.Path);
			if (pathParameters == null)
				return null;

			var context = new ServiceHttpContext();
			ServiceHttpContext.SetContext(httpRequest, context);

			var aspectHttpResponse = await AdaptTask(RequestReceivedAsync(httpRequest, cancellationToken)).ConfigureAwait(true);
			if (aspectHttpResponse != null)
				return aspectHttpResponse;

			string mediaType = GetResponseMediaType(httpRequest);

			ServiceErrorDto error = null;

			ServiceDto requestBody = null;
			if (mapping.RequestBodyType != null)
			{
				var requestResult = await AdaptTask(HttpServiceUtility.TryReadHttpContentAsync(mapping.RequestBodyType, httpRequest.Content, ServiceErrors.InvalidRequest)).ConfigureAwait(true);
				if (requestResult.IsFailure)
					error = requestResult.Error;
				else
					requestBody = requestResult.Value ?? (ServiceDto) Activator.CreateInstance(mapping.RequestBodyType);
			}

			TResponse response = null;
			if (error == null)
			{
				var request = mapping.CreateRequest(requestBody);

				var uriParameters = new Dictionary<string, string>();
				foreach (var queryParameter in HttpServiceUtility.ParseQueryString(httpRequest.RequestUri.Query))
					uriParameters[queryParameter.Key] = queryParameter.Value[0];
				foreach (var pathParameter in pathParameters)
					uriParameters[pathParameter.Key] = pathParameter.Value;
				request = mapping.SetUriParameters(request, uriParameters);
				request = mapping.SetRequestHeaders(request, HttpServiceUtility.CreateDictionaryFromHeaders(httpRequest.Headers));

				context.Request = request;

				var methodResult = await invokeMethodAsync(request, cancellationToken).ConfigureAwait(true);
				if (methodResult.IsFailure)
					error = methodResult.Error;
				else
					response = methodResult.Value;

				context.Result = error != null ? ServiceResult.Failure(error) : ServiceResult.Success<ServiceDto>(response);
			}

			HttpResponseMessage httpResponse = null;
			if (error == null)
			{
				var responseMapping = (
					from rm in mapping.ResponseMappings
					let matches = rm.MatchesResponse(response)
					where matches != false
					orderby matches descending
					select rm).FirstOrDefault();
				if (responseMapping != null)
				{
					httpResponse = new HttpResponseMessage(responseMapping.StatusCode);

					var headersResult = HttpServiceUtility.TryAddHeaders(httpResponse.Headers, mapping.GetResponseHeaders(response));
					if (headersResult.IsFailure)
					{
						error = headersResult.Error;
					}
					else if (responseMapping.ResponseBodyType != null)
					{
						httpResponse.Content = await AdaptTask(HttpServiceUtility.CreateHttpContentAsync(
							responseMapping.GetResponseBody(response), mediaType)).ConfigureAwait(true);
					}
				}
			}

			if (httpResponse == null)
				httpResponse = await AdaptTask(HttpServiceUtility.CreateHttpResponseFromErrorAsync(error ?? ServiceErrors.CreateInvalidResponse(), mediaType, TryGetCustomHttpStatusCode)).ConfigureAwait(true);

			httpResponse.RequestMessage = httpRequest;
			await AdaptTask(ResponseReadyAsync(httpResponse, context.Result, cancellationToken)).ConfigureAwait(true);

			return httpResponse;
		}

		/// <summary>
		/// Returns the HTTP status code for a custom error code.
		/// </summary>
		protected virtual HttpStatusCode? TryGetCustomHttpStatusCode(string errorCode)
		{
			return null;
		}

		/// <summary>
		/// Handle or delegate the HTTP request.
		/// </summary>
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return await TryHandleHttpRequestAsync(request, cancellationToken).ConfigureAwait(true) ??
				await base.SendAsync(request, cancellationToken).ConfigureAwait(true);
		}

		readonly bool m_synchronous;
		readonly string m_defaultMediaType;
		readonly IReadOnlyList<ServiceHttpHandlerAspect> m_aspects;
	}
}
