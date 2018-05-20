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
		public string TestName { get; set; }

		/// <summary>
		/// The API method being called.
		/// </summary>
		public string Method { get; set; }

		/// <summary>
		/// The API request being sent.
		/// </summary>
		public JObject Request { get; set; }

		/// <summary>
		/// The API response being received, if the method should succeed.
		/// </summary>
		public JObject Response { get; set; }

		/// <summary>
		/// The API error being received, if the method should fail.
		/// </summary>
		public JObject Error { get; set; }
	}
}
