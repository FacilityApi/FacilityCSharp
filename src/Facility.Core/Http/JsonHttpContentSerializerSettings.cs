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
	}
}
