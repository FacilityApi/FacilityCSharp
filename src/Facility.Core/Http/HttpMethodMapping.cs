using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;

namespace Facility.Core.Http
{
	/// <summary>
	/// Defines the HTTP mapping for a service method.
	/// </summary>
	public sealed class HttpMethodMapping<TRequest, TResponse>
		where TRequest : ServiceDto, new()
		where TResponse : ServiceDto, new()
	{
		/// <summary>
		/// The HTTP method.
		/// </summary>
		public HttpMethod HttpMethod { get; }

		/// <summary>
		/// The path.
		/// </summary>
		public string Path { get; }

		/// <summary>
		/// Returns an error if the request is invalid.
		/// </summary>
		public ServiceResult ValidateRequest(TRequest request)
		{
			return m_validateRequest?.Invoke(request) ?? ServiceResult.Success();
		}

		/// <summary>
		/// Extracts the path and query parameters from the request.
		/// </summary>
		public IReadOnlyDictionary<string, string> GetUriParameters(TRequest request)
		{
			return m_getUriParameters?.Invoke(request);
		}

		/// <summary>
		/// Writes the path and query parameters to the request.
		/// </summary>
		public TRequest SetUriParameters(TRequest request, IReadOnlyDictionary<string, string> uriParameters)
		{
			return m_setUriParameters?.Invoke(request, uriParameters) ?? request;
		}

		/// <summary>
		/// The type of the request body, if any.
		/// </summary>
		public Type RequestBodyType { get; }

		/// <summary>
		/// Extracts the HTTP request content body from the request.
		/// </summary>
		public ServiceDto GetRequestBody(TRequest request)
		{
			return m_getRequestBody != null ? m_getRequestBody(request) : RequestBodyType == typeof(TRequest) ? request : null;
		}

		/// <summary>
		/// Extracts the headers from the request.
		/// </summary>
		public IReadOnlyDictionary<string, string> GetRequestHeaders(TRequest request)
		{
			return m_getRequestHeaders?.Invoke(request);
		}

		/// <summary>
		/// Writes the headers to the request.
		/// </summary>
		public TRequest SetRequestHeaders(TRequest request, IReadOnlyDictionary<string, string> requestHeaders)
		{
			return m_setRequestHeaders?.Invoke(request, requestHeaders) ?? request;
		}

		/// <summary>
		/// Creates a request with an optional body.
		/// </summary>
		public TRequest CreateRequest(ServiceDto requestBody)
		{
			if (m_createRequest != null)
				return m_createRequest(requestBody);
			else if (RequestBodyType == typeof(TRequest))
				return (TRequest) requestBody;
			else
				return new TRequest();
		}

		/// <summary>
		/// The response mappings.
		/// </summary>
		public IReadOnlyList<HttpResponseMapping<TResponse>> ResponseMappings { get; }

		/// <summary>
		/// Extracts the headers from the response.
		/// </summary>
		public IReadOnlyDictionary<string, string> GetResponseHeaders(TResponse response)
		{
			return m_getResponseHeaders?.Invoke(response);
		}

		/// <summary>
		/// Writes the headers to the response.
		/// </summary>
		public TResponse SetResponseHeaders(TResponse response, IReadOnlyDictionary<string, string> responseHeaders)
		{
			return m_setResponseHeaders?.Invoke(response, responseHeaders) ?? response;
		}

		/// <summary>
		/// Used to build instances of this class.
		/// </summary>
		public sealed class Builder
		{
			public HttpMethod HttpMethod { get; set; }

			public string Path { get; set; }

			public Func<TRequest, ServiceResult> ValidateRequest { get; set; }

			public Func<TRequest, IReadOnlyDictionary<string, string>> GetUriParameters { get; set; }

			public Func<TRequest, IReadOnlyDictionary<string, string>, TRequest> SetUriParameters { get; set; }

			public Type RequestBodyType { get; set; }

			public Func<TRequest, ServiceDto> GetRequestBody { get; set; }

			public Func<TRequest, IReadOnlyDictionary<string, string>> GetRequestHeaders { get; set; }

			public Func<TRequest, IReadOnlyDictionary<string, string>, TRequest> SetRequestHeaders { get; set; }

			public Func<ServiceDto, TRequest> CreateRequest { get; set; }

			public Collection<HttpResponseMapping<TResponse>> ResponseMappings { get; } = new Collection<HttpResponseMapping<TResponse>>();

			public Func<TResponse, IReadOnlyDictionary<string, string>> GetResponseHeaders { get; set; }

			public Func<TResponse, IReadOnlyDictionary<string, string>, TResponse> SetResponseHeaders { get; set; }

			public HttpMethodMapping<TRequest, TResponse> Build()
			{
				return new HttpMethodMapping<TRequest, TResponse>(this);
			}
		}

		private HttpMethodMapping(Builder builder)
		{
			HttpMethod = builder.HttpMethod;
			Path = builder.Path;
			m_validateRequest = builder.ValidateRequest;
			m_getUriParameters = builder.GetUriParameters;
			m_setUriParameters = builder.SetUriParameters;
			RequestBodyType = builder.RequestBodyType;
			m_getRequestBody = builder.GetRequestBody;
			m_getRequestHeaders = builder.GetRequestHeaders;
			m_setRequestHeaders = builder.SetRequestHeaders;
			m_createRequest = builder.CreateRequest;
			m_getResponseHeaders = builder.GetResponseHeaders;
			m_setResponseHeaders = builder.SetResponseHeaders;
			ResponseMappings = builder.ResponseMappings.ToList();
		}

		readonly Func<TRequest, ServiceResult> m_validateRequest;
		readonly Func<TRequest, IReadOnlyDictionary<string, string>> m_getUriParameters;
		readonly Func<TRequest, IReadOnlyDictionary<string, string>, TRequest> m_setUriParameters;
		readonly Func<TRequest, IReadOnlyDictionary<string, string>> m_getRequestHeaders;
		readonly Func<TRequest, IReadOnlyDictionary<string, string>, TRequest> m_setRequestHeaders;
		readonly Func<TRequest, ServiceDto> m_getRequestBody;
		readonly Func<ServiceDto, TRequest> m_createRequest;
		readonly Func<TResponse, IReadOnlyDictionary<string, string>> m_getResponseHeaders;
		readonly Func<TResponse, IReadOnlyDictionary<string, string>, TResponse> m_setResponseHeaders;
	}
}
