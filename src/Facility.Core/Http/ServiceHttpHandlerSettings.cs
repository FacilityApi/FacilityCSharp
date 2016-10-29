using System.Collections.Generic;

namespace Facility.Core.Http
{
	/// <summary>
	/// Settings for service HTTP handlers.
	/// </summary>
	public class ServiceHttpHandlerSettings
	{
		/// <summary>
		/// The root path of the service, default "/".
		/// </summary>
		public string RootPath { get; set; }

		/// <summary>
		/// True to call services synchronously, allowing tasks to be safely blocked.
		/// </summary>
		public bool Synchronous { get; set; }

		/// <summary>
		/// The default media type used by responses, default "application/json".
		/// </summary>
		public string DefaultMediaType { get; set; }

		/// <summary>
		/// The aspects used when receiving requests and sending responses.
		/// </summary>
		public IReadOnlyList<ServiceHttpHandlerAspect> Aspects { get; set; }
	}
}
