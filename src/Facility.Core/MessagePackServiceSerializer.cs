using MessagePack;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values using MessagePack.
/// </summary>
public sealed class MessagePackServiceSerializer : ServiceSerializer
{
	/// <summary>
	/// The serializer instance.
	/// </summary>
	public static readonly MessagePackServiceSerializer Instance = new();

	/// <summary>
	/// The default media type.
	/// </summary>
	public override string DefaultMediaType => "application/x-msgpack";

	/// <summary>
	/// Serializes a value to the serialization format.
	/// </summary>
	public override void ToStream(object? value, Stream stream)
	{
		try
		{
			MessagePackSerializer.Serialize(stream, value, s_serializerOptions);
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
	public override object? FromStream(Stream stream, Type type)
	{
		try
		{
			return MessagePackSerializer.Deserialize(type, stream, s_serializerOptions);
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	public override T Clone<T>(T value)
	{
		if (value is null)
			return default!;

		try
		{
			return MessagePackSerializer.Deserialize<T>(MessagePackSerializer.Serialize(value, s_serializerOptions));
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	private MessagePackServiceSerializer()
	{
	}

	private static readonly MessagePackSerializerOptions s_serializerOptions = MessagePackSerializerOptions.Standard;
}
