using System.Buffers;
using MessagePack;
using MessagePack.Formatters;

namespace Facility.Core.MessagePack;

internal sealed class ServiceObjectMessagePackFormatter : IMessagePackFormatter<ServiceObject>
{
	public static readonly ServiceObjectMessagePackFormatter Instance = new();

	public void Serialize(ref MessagePackWriter writer, ServiceObject? value, MessagePackSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNil();
		}
		else
		{
			using var stream = new MemoryStream();
			SystemTextJsonServiceSerializer.Instance.ToStream(value.AsJsonObject(ServiceObjectAccess.ReadOnly), stream);
			writer.WriteString(stream.GetBuffer().AsSpan(0, (int) stream.Length));
		}
	}

	public ServiceObject Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		var jsonBytes = reader.ReadStringSequence();
		if (jsonBytes is null)
			return null!;

		using var stream = new MemoryStream(jsonBytes.Value.ToArray());
		return (ServiceObject) SystemTextJsonServiceSerializer.Instance.FromStream(stream, typeof(ServiceObject))!;
	}
}
