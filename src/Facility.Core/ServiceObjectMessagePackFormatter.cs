using System.Buffers;
using MessagePack;
using MessagePack.Formatters;

namespace Facility.Core;

public class ServiceObjectMessagePackFormatter : IMessagePackFormatter<ServiceObject>
{
	public void Serialize(ref MessagePackWriter writer, ServiceObject? value, MessagePackSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNil();
		}
		else
		{
			var json = SystemTextJsonServiceSerializer.Instance.ToBytes(value.AsJsonObject());
			writer.WriteString(json);
		}
	}

	public ServiceObject Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		var json = reader.ReadStringSequence();
		return json is null ? null! : SystemTextJsonServiceSerializer.Instance.FromBytes<ServiceObject>(json.Value.ToArray())!;
	}
}
