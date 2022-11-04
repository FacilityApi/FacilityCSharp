using MessagePack;
using MessagePack.Formatters;

namespace Facility.Core.MessagePack;

public sealed class ServiceFieldMessagePackFormatter<T> : IMessagePackFormatter<ServiceField<T>>
{
	public void Serialize(ref MessagePackWriter writer, ServiceField<T> value, MessagePackSerializerOptions options)
	{
		if (value.IsDefault)
		{
			writer.WriteNil();
			return;
		}

		writer.WriteArrayHeader(1);

		options.Resolver.GetFormatterWithVerify<T?>().Serialize(ref writer, value.Value, options);
	}

	public ServiceField<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
			return default;

		var count = reader.ReadArrayHeader();
		if (count != 1)
			throw new MessagePackSerializationException("Invalid ServiceField format.");

		options.Security.DepthStep(ref reader);
		try
		{
			return options.Resolver.GetFormatterWithVerify<T?>().Deserialize(ref reader, options);
		}
		finally
		{
			reader.Depth--;
		}
	}
}
