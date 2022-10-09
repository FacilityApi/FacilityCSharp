using MessagePack;
using MessagePack.Formatters;

namespace Facility.Core.MessagePack;

internal sealed class ServiceErrorMessagePackFormatter : IMessagePackFormatter<ServiceErrorDto>
{
	public static readonly ServiceErrorMessagePackFormatter Instance = new();

	public void Serialize(ref MessagePackWriter writer, ServiceErrorDto? value, MessagePackSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNil();
			return;
		}

		writer.WriteArrayHeader(4);

		var resolver = options.Resolver;
		resolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.Code, options);
		resolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.Message, options);
		resolver.GetFormatterWithVerify<ServiceObject?>().Serialize(ref writer, value.DetailsObject, options);
		resolver.GetFormatterWithVerify<ServiceErrorDto?>().Serialize(ref writer, value.InnerError, options);
	}

	public ServiceErrorDto Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
			return null!;

		var count = reader.ReadArrayHeader();
		if (count != 4)
			throw new MessagePackSerializationException("Invalid ServiceErrorDto format.");

		var resolver = options.Resolver;
		options.Security.DepthStep(ref reader);
		try
		{
			return new ServiceErrorDto
			{
				Code = resolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, options),
				Message = resolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, options),
				DetailsObject = resolver.GetFormatterWithVerify<ServiceObject?>().Deserialize(ref reader, options),
				InnerError = resolver.GetFormatterWithVerify<ServiceErrorDto?>().Deserialize(ref reader, options),
			};
		}
		finally
		{
			reader.Depth--;
		}
	}
}
