using System;
using System.IO;

namespace Facility.Core.Http
{
	/// <summary>
	/// Settings for <see cref="JsonHttpContentSerializer" />.
	/// </summary>
	public class JsonHttpContentSerializerSettings
	{
		/// <summary>
		/// True to force async I/O, even if a large memory buffer is required.
		/// </summary>
		public bool ForceAsyncIO { get; set; }

		/// <summary>
		/// Called to create a memory stream. Defaults to creating a new <see cref="MemoryStream" />.
		/// </summary>
		/// <remarks>Consider using <c>Microsoft.IO.RecyclableMemoryStream</c>.</remarks>
		public Func<Stream>? MemoryStreamCreator { get; set; }
	}
}
