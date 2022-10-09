using MessagePack;
using MessagePack.Formatters;

namespace Facility.Core.MessagePack;

/// <summary>
/// Used by MessagePack to convert string-based enumerated types.
/// </summary>
public abstract class ServiceEnumMessagePackFormatter<T> : IMessagePackFormatter<T?>
{
	/// <summary>
	/// Serializes a value.
	/// </summary>
	public void Serialize(ref MessagePackWriter writer, T? value, MessagePackSerializerOptions options) =>
		writer.Write(value?.ToString());

	/// <summary>
	/// Deserializes a value.
	/// </summary>
	public T? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
		reader.ReadString() is { } text ? CreateCore(text) : default;

	/// <summary>
	/// Creates the value from a string.
	/// </summary>
	protected abstract T CreateCore(string value);
}
