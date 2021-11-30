using System.Text.Json;
using System.Text.Json.Serialization;

namespace Facility.Core;

/// <summary>
/// Used to JSON-serialize string-based enumerated types.
/// </summary>
public abstract class ServiceEnumSystemTextJsonConverter<T> : JsonConverter<T>
	where T : struct
{
	/// <summary>
	/// Creates the value from a string.
	/// </summary>
	protected abstract T CreateCore(string value);

	/// <summary>
	/// Reads the JSON representation of the value.
	/// </summary>
	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
			throw new JsonException($"Expected string for {typeof(T).Name}; got {reader.TokenType}.");

		return CreateCore(reader.GetString()!);
	}

	/// <summary>
	/// Writes the JSON representation of the value.
	/// </summary>
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
		writer.WriteStringValue(value.ToString());
}
