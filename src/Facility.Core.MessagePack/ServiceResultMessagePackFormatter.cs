using System.Diagnostics.CodeAnalysis;
using MessagePack;
using MessagePack.Formatters;

namespace Facility.Core.MessagePack;

internal sealed class ServiceResultMessagePackFormatter : IMessagePackFormatter<ServiceResult>
{
	public static readonly ServiceResultMessagePackFormatter Instance = new();

	public void Serialize(ref MessagePackWriter writer, ServiceResult? value, MessagePackSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNil();
			return;
		}

		writer.WriteArrayHeader(2);

		var resolver = options.Resolver;
		writer.WriteNil();
		resolver.GetFormatterWithVerify<ServiceErrorDto>().Serialize(ref writer, value.Error!, options);
	}

	public ServiceResult Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
			return null!;

		var count = reader.ReadArrayHeader();
		if (count != 2)
			throw new MessagePackSerializationException("Invalid ServiceResult format.");

		var resolver = options.Resolver;
		options.Security.DepthStep(ref reader);
		try
		{
			if (!reader.TryReadNil())
				throw new MessagePackSerializationException("ServiceResult does not support 'value'; use ServiceResult<T>.");

			var error = resolver.GetFormatterWithVerify<ServiceErrorDto>().Deserialize(ref reader, options);
			return error is null ? ServiceResult.Success() : ServiceResult.Failure(error);
		}
		finally
		{
			reader.Depth--;
		}
	}
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
public sealed class ServiceResultMessagePackFormatter<T> : IMessagePackFormatter<ServiceResult<T>>
{
	public void Serialize(ref MessagePackWriter writer, ServiceResult<T>? value, MessagePackSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNil();
			return;
		}

		writer.WriteArrayHeader(2);

		var resolver = options.Resolver;
		resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value.GetValueOrDefault()!, options);
		resolver.GetFormatterWithVerify<ServiceErrorDto>().Serialize(ref writer, value.Error!, options);
	}

	public ServiceResult<T> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
			return null!;

		var count = reader.ReadArrayHeader();
		if (count != 2)
			throw new MessagePackSerializationException("Invalid ServiceResult format.");

		var resolver = options.Resolver;
		options.Security.DepthStep(ref reader);
		try
		{
			var value = resolver.GetFormatterWithVerify<T>().Deserialize(ref reader, options);
			var error = resolver.GetFormatterWithVerify<ServiceErrorDto>().Deserialize(ref reader, options);
			return error is null ? ServiceResult.Success(value) : ServiceResult.Failure(error);
		}
		finally
		{
			reader.Depth--;
		}
	}
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
internal sealed class ServiceResultFailureMessagePackFormatter : IMessagePackFormatter<ServiceResultFailure>
{
	public static readonly ServiceResultFailureMessagePackFormatter Instance = new();

	public void Serialize(ref MessagePackWriter writer, ServiceResultFailure? value, MessagePackSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNil();
			return;
		}

		writer.WriteArrayHeader(2);

		var resolver = options.Resolver;
		writer.WriteNil();
		resolver.GetFormatterWithVerify<ServiceErrorDto>().Serialize(ref writer, value.Error!, options);
	}

	public ServiceResultFailure Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
			return null!;

		var count = reader.ReadArrayHeader();
		if (count != 2)
			throw new MessagePackSerializationException("Invalid ServiceResult format.");

		var resolver = options.Resolver;
		options.Security.DepthStep(ref reader);
		try
		{
			if (!reader.TryReadNil())
				throw new MessagePackSerializationException("ServiceResult does not support 'value'; use ServiceResult<T>.");

			var error = resolver.GetFormatterWithVerify<ServiceErrorDto>().Deserialize(ref reader, options);
			if (error is null)
				throw new MessagePackSerializationException("ServiceResultFailure does not support success.");

			return ServiceResult.Failure(error);
		}
		finally
		{
			reader.Depth--;
		}
	}
}
