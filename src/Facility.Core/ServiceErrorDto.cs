using Newtonsoft.Json.Linq;
using ProtoBuf;

namespace Facility.Core;

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

	/// <summary>
	/// Advanced error details.
	/// </summary>
	/// <remarks>Obsolete; use <see cref="DetailsObject" />.</remarks>
	[Obsolete("Use DetailsObject.")]
	[Newtonsoft.Json.JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	[ProtoIgnore]
	public JObject? Details
	{
		get => DetailsObject?.AsJObject();
		set => DetailsObject = ServiceObject.Create(value);
	}
}
