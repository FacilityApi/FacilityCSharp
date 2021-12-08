using System.Buffers;
using MessagePack;
using MessagePack.Formatters;

namespace Facility.Core;

/// <summary>
/// Used by MessagePack to convert <see cref="ServiceObject" />.
/// </summary>
public sealed class ServiceObjectMessagePackFormatter : IMessagePackFormatter<ServiceObject>
{
	/// <summary>
	/// Serializes a value.
	/// </summary>
	public void Serialize(ref MessagePackWriter writer, ServiceObject? value, MessagePackSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNil();
		}
		else
		{
			using var stream = new MemoryStream();
			SystemTextJsonServiceSerializer.Instance.ToStream(value.AsJsonObject(), stream);
			writer.WriteString(stream.GetBuffer().AsSpan(0, (int) stream.Length));
		}
	}

	/// <summary>
	/// Deserializes a value.
	/// </summary>
	public ServiceObject Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		var jsonBytes = reader.ReadStringSequence();
		if (jsonBytes is null)
			return null!;

		using var stream = new MemoryStream(jsonBytes.Value.ToArray());
		return (ServiceObject) SystemTextJsonServiceSerializer.Instance.FromStream(stream, typeof(ServiceObject))!;
	}
}
