using Newtonsoft.Json.Linq;

namespace Facility.ConformanceApi.Testing
{
	/// <summary>
	/// Information for a conformance test.
	/// </summary>
	public sealed class ConformanceTestInfo
	{
		/// <summary>
		/// The name of the test.
		/// </summary>
		public string? Test { get; set; }

		/// <summary>
		/// The API method being called.
		/// </summary>
		public string? Method { get; set; }

		/// <summary>
		/// The API request being sent.
		/// </summary>
		public JObject? Request { get; set; }

		/// <summary>
		/// The API response being received, if the method should succeed.
		/// </summary>
		public JObject? Response { get; set; }

		/// <summary>
		/// The API error being received, if the method should fail.
		/// </summary>
		public JObject? Error { get; set; }

		/// <summary>
		/// A raw HTTP request.
		/// </summary>
		public ConformanceHttpRequestInfo? HttpRequest { get; set; }
	}
}
