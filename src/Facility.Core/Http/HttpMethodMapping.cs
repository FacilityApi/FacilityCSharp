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
		/// Returns a failure if the request is invalid.
		/// </summary>
		public ServiceResult ValidateRequest(TRequest request) =>
			m_validateRequest?.Invoke(request) ?? ServiceResult.Success();

		/// <summary>
		/// Extracts the path and query parameters from the request.
		/// </summary>
		public IReadOnlyDictionary<string, string?>? GetUriParameters(TRequest request) =>
			m_getUriParameters?.Invoke(request);

		/// <summary>
		/// Writes the path and query parameters to the request.
		/// </summary>
		public TRequest SetUriParameters(TRequest request, IReadOnlyDictionary<string, string?> uriParameters) =>
			m_setUriParameters?.Invoke(request, uriParameters) ?? request;

		/// <summary>
		/// The type of the request body, if any.
		/// </summary>
		public Type? RequestBodyType { get; }

		/// <summary>
		/// Extracts the HTTP request content body from the request.
		/// </summary>
		public object? GetRequestBody(TRequest request) =>
			m_getRequestBody != null ? m_getRequestBody(request) : RequestBodyType == typeof(TRequest) ? request : null;

		/// <summary>
		/// Extracts the headers from the request.
		/// </summary>
		public IReadOnlyDictionary<string, string?>? GetRequestHeaders(TRequest request) =>
			m_getRequestHeaders?.Invoke(request);

		/// <summary>
		/// Writes the headers to the request.
		/// </summary>
		public TRequest SetRequestHeaders(TRequest request, IReadOnlyDictionary<string, string?> requestHeaders) =>
			m_setRequestHeaders?.Invoke(request, requestHeaders) ?? request;

		/// <summary>
		/// Creates a request with an optional body.
		/// </summary>
		public TRequest CreateRequest(object? requestBody) =>
			m_createRequest?.Invoke(requestBody) ?? requestBody as TRequest ?? new TRequest();

		/// <summary>
		/// The response mappings.
		/// </summary>
		public IReadOnlyList<HttpResponseMapping<TResponse>> ResponseMappings { get; }

		/// <summary>
		/// Extracts the headers from the response.
		/// </summary>
		public IReadOnlyDictionary<string, string?>? GetResponseHeaders(TResponse response) =>
			m_getResponseHeaders?.Invoke(response);

		/// <summary>
		/// Writes the headers to the response.
		/// </summary>
		public TResponse SetResponseHeaders(TResponse response, IReadOnlyDictionary<string, string?> responseHeaders) =>
			m_setResponseHeaders?.Invoke(response, responseHeaders) ?? response;

		/// <summary>
		/// Used to build instances of this class.
		/// </summary>
		public sealed class Builder
		{
			/// <summary>
			/// The HTTP method.
			/// </summary>
			public HttpMethod? HttpMethod { get; set; }

			/// <summary>
			/// The path.
			/// </summary>
			public string? Path { get; set; }

			/// <summary>
			/// Returns an error if the request is invalid.
			/// </summary>
			public Func<TRequest, ServiceResult>? ValidateRequest { get; set; }

			/// <summary>
			/// Extracts the path and query parameters from the request.
			/// </summary>
			public Func<TRequest, IReadOnlyDictionary<string, string?>>? GetUriParameters { get; set; }

			/// <summary>
			/// Writes the path and query parameters to the request.
			/// </summary>
			public Func<TRequest, IReadOnlyDictionary<string, string?>, TRequest>? SetUriParameters { get; set; }

			/// <summary>
			/// The type of the request body, if any.
			/// </summary>
			public Type? RequestBodyType { get; set; }

			/// <summary>
			/// Extracts the HTTP request content body from the request.
			/// </summary>
			public Func<TRequest, object?>? GetRequestBody { get; set; }

			/// <summary>
			/// Extracts the headers from the request.
			/// </summary>
			public Func<TRequest, IReadOnlyDictionary<string, string?>>? GetRequestHeaders { get; set; }

			/// <summary>
			/// Writes the headers to the request.
			/// </summary>
			public Func<TRequest, IReadOnlyDictionary<string, string?>, TRequest>? SetRequestHeaders { get; set; }

			/// <summary>
			/// Creates a request with an optional body.
			/// </summary>
			public Func<object?, TRequest>? CreateRequest { get; set; }

			/// <summary>
			/// The response mappings.
			/// </summary>
			// ReSharper disable once CollectionNeverUpdated.Global (used in generated code)
			public Collection<HttpResponseMapping<TResponse>> ResponseMappings { get; } = new();

			/// <summary>
			/// Extracts the headers from the response.
			/// </summary>
			public Func<TResponse, IReadOnlyDictionary<string, string?>>? GetResponseHeaders { get; set; }

			/// <summary>
			/// Writes the headers to the response.
			/// </summary>
			public Func<TResponse, IReadOnlyDictionary<string, string?>, TResponse>? SetResponseHeaders { get; set; }

			/// <summary>
			/// Builds the mapping.
			/// </summary>
			public HttpMethodMapping<TRequest, TResponse> Build() => new(this);
		}

		private HttpMethodMapping(Builder builder)
		{
			HttpMethod = builder.HttpMethod ?? throw new InvalidOperationException("HttpMethod must be specified.");
			Path = builder.Path ?? throw new InvalidOperationException("Path must be specified.");
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

		private readonly Func<TRequest, ServiceResult>? m_validateRequest;
		private readonly Func<TRequest, IReadOnlyDictionary<string, string?>>? m_getUriParameters;
		private readonly Func<TRequest, IReadOnlyDictionary<string, string?>, TRequest>? m_setUriParameters;
		private readonly Func<TRequest, IReadOnlyDictionary<string, string?>>? m_getRequestHeaders;
		private readonly Func<TRequest, IReadOnlyDictionary<string, string?>, TRequest>? m_setRequestHeaders;
		private readonly Func<TRequest, object?>? m_getRequestBody;
		private readonly Func<object?, TRequest>? m_createRequest;
		private readonly Func<TResponse, IReadOnlyDictionary<string, string?>>? m_getResponseHeaders;
		private readonly Func<TResponse, IReadOnlyDictionary<string, string?>, TResponse>? m_setResponseHeaders;
	}
}
