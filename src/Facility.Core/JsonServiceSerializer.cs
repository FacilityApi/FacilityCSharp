using System.Diagnostics.CodeAnalysis;
using Facility.Core.Http;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values to and from JSON.
/// </summary>
public abstract class JsonServiceSerializer : ServiceSerializer
{
	/// <summary>
	/// The media type used by default.
	/// </summary>
	public override string DefaultMediaType => HttpServiceUtility.JsonMediaType;

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public abstract string ToJson(object? value);

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
	public abstract object? FromJson(string json, Type type);

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
	public virtual T? FromJson<T>(string json) => (T?) FromJson(json, typeof(T));

	/// <summary>
	/// Serializes a value to a <see cref="ServiceObject"/> representation of JSON.
	/// </summary>
	[return: NotNullIfNotNull(nameof(value))]
	public abstract ServiceObject? ToServiceObject(object? value);

	/// <summary>
	/// Deserializes a value from a <see cref="ServiceObject"/> representation of JSON.
	/// </summary>
	[return: NotNullIfNotNull(nameof(serviceObject))]
	public abstract object? FromServiceObject(ServiceObject? serviceObject, Type type);

	/// <summary>
	/// Deserializes a value from a <see cref="ServiceObject"/> representation of JSON.
	/// </summary>
	[return: NotNullIfNotNull(nameof(serviceObject))]
	public virtual T? FromServiceObject<T>(ServiceObject? serviceObject) => (T?) FromServiceObject(serviceObject, typeof(T));

	/// <summary>
	/// The legacy serializer, <see cref="NewtonsoftJsonServiceSerializer"/>.
	/// </summary>
	/// <remarks>This must be used by default to support older client libraries that don't support
	/// <c>System.Text.Json</c>.</remarks>
	internal static JsonServiceSerializer Legacy => NewtonsoftJsonServiceSerializer.Instance;
}
