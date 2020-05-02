using System;
using System.Net;

namespace Facility.Core.Http
{
	/// <summary>
	/// Defines the HTTP mapping for a service method response.
	/// </summary>
	public sealed class HttpResponseMapping<TResponse>
		where TResponse : ServiceDto, new()
	{
		/// <summary>
		/// The status code used by this mapping.
		/// </summary>
		public HttpStatusCode StatusCode { get; }

		/// <summary>
		/// True if the response should result in this status code and body.
		/// </summary>
		public bool? MatchesResponse(TResponse response) => m_matchesResponse?.Invoke(response);

		/// <summary>
		/// The type of the response body, if any.
		/// </summary>
		public Type ResponseBodyType { get; }

		/// <summary>
		/// Extracts the HTTP response content body from the response.
		/// </summary>
		public object? GetResponseBody(TResponse response) =>
			m_getResponseBody != null ? m_getResponseBody(response) : ResponseBodyType == typeof(TResponse) ? response : null;

		/// <summary>
		/// Creates a response with an optional body.
		/// </summary>
		public TResponse CreateResponse(object? responseBody) =>
			m_createResponse?.Invoke(responseBody) ?? responseBody as TResponse ?? new TResponse();

		/// <summary>
		/// Used to build instances of this class.
		/// </summary>
		public sealed class Builder
		{
			/// <summary>
			/// The status code used by this mapping.
			/// </summary>
			public HttpStatusCode? StatusCode { get; set; }

			/// <summary>
			/// True if the response should result in this status code and body.
			/// </summary>
			public Func<TResponse, bool>? MatchesResponse { get; set; }

			/// <summary>
			/// The type of the response body, if any.
			/// </summary>
			public Type? ResponseBodyType { get; set; }

			/// <summary>
			/// Extracts the HTTP response content body from the response.
			/// </summary>
			public Func<TResponse, object>? GetResponseBody { get; set; }

			/// <summary>
			/// Creates a response with an optional body.
			/// </summary>
			public Func<object?, TResponse>? CreateResponse { get; set; }

			/// <summary>
			/// Builds the mapping.
			/// </summary>
			public HttpResponseMapping<TResponse> Build() => new HttpResponseMapping<TResponse>(this);
		}

		private HttpResponseMapping(Builder builder)
		{
			StatusCode = builder.StatusCode ?? throw new InvalidOperationException("StatusCode is required.");
			ResponseBodyType = builder.ResponseBodyType!;
			m_matchesResponse = builder.MatchesResponse!;
			m_getResponseBody = builder.GetResponseBody!;
			m_createResponse = builder.CreateResponse!;
		}

		readonly Func<TResponse, bool> m_matchesResponse;
		readonly Func<TResponse, object> m_getResponseBody;
		readonly Func<object?, TResponse> m_createResponse;
	}
}
