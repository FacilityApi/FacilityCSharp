using System.Reflection;
using Newtonsoft.Json;

namespace Facility.Core;

/// <summary>
/// Base class for simple JSON converters.
/// </summary>
/// <typeparam name="T">The type.</typeparam>
/// <remarks>This base class provides strongly-typed abstract methods for reading and writing. The converter only
/// supports type T directly, not derived types. Null is handled automatically for both reading and writing, including support
/// for Nullable{T}. A JsonSerializationException is thrown if null is read for a non-nullable value type. If your converter
/// has more advanced needs, derive from JsonConverter directly.</remarks>
public abstract class ServiceJsonConverterBase<T> : JsonConverter
{
	/// <summary>
	/// Implements CanConvert.
	/// </summary>
	public override bool CanConvert(Type objectType) => objectType == typeof(T) || (s_nullableType != null && objectType == s_nullableType);

	/// <summary>
	/// Implements ReadJson.
	/// </summary>
	public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			if (s_nullableType != null && objectType != s_nullableType)
				throw new JsonSerializationException("Cannot convert null to non-nullable type " + objectType.Name);
			return null;
		}

		return ReadCore(reader, serializer);
	}

	/// <summary>
	/// Implements WriteJson.
	/// </summary>
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => WriteCore(writer, (T) value, serializer);

	/// <summary>
	/// Reads the JSON representation of the value.
	/// </summary>
	protected abstract T ReadCore(JsonReader reader, JsonSerializer serializer);

	/// <summary>
	/// Writes the JSON representation of the value.
	/// </summary>
	protected abstract void WriteCore(JsonWriter writer, T value, JsonSerializer serializer);

	private static readonly Type? s_nullableType = typeof(T).GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null ? typeof(Nullable<>).MakeGenericType(typeof(T)) : null;
}
