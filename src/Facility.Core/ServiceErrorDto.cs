using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.Core
{
	/// <summary>
	/// An error.
	/// </summary>
	public sealed partial class ServiceErrorDto
	{
		/// <summary>
		/// Creates a service error.
		/// </summary>
		public ServiceErrorDto(string? code)
		{
			Code = code;
		}

		/// <summary>
		/// Creates a service error.
		/// </summary>
		public ServiceErrorDto(string? code, string? message)
		{
			Code = code;
			Message = message;
		}

		[Obsolete("Use DetailsObject.")]
		[JsonIgnore]
		public JObject? Details
		{
			get => DetailsObject?.AsJObject();
			set => DetailsObject = ServiceObject.Create(value);
		}
	}
}
