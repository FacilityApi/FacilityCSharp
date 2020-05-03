using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Facility.Core.Http
{
	/// <summary>
	/// Settings for HTTP client services.
	/// </summary>
	public sealed class HttpClientServiceSettings
	{
		/// <summary>
		/// The base URI of the service (optional if the service has a default base URI).
		/// </summary>
		public Uri? BaseUri { get; set; }

		/// <summary>
		/// The HttpClient to use (optional).
		/// </summary>
		public HttpClient? HttpClient { get; set; }

		/// <summary>
		/// The content serializer used by requests and responses (optional).
		/// </summary>
		public HttpContentSerializer? ContentSerializer { get; set; }

		/// <summary>
		/// The aspects used when sending requests and receiving responses (optional).
		/// </summary>
		public IReadOnlyList<HttpClientServiceAspect>? Aspects { get; set; }

		/// <summary>
		/// True to call services synchronously, allowing tasks to be safely blocked.
		/// </summary>
		public bool Synchronous { get; set; }

		/// <summary>
		/// True to prevent the validation of request DTOs before sending.
		/// </summary>
		public bool SkipRequestValidation { get; set; }
	}
}
