using MessagePack;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values to and from JSON using <c>System.Text.Json</c>.
/// </summary>
public sealed class MessagePackServiceSerializer : ServiceSerializer
{
	/// <summary>
	/// The serializer instance.
	/// </summary>
	public static readonly MessagePackServiceSerializer Instance = new();

	public override string DefaultMediaType => "application/x-msgpack";

	/// <summary>
	/// Serializes a value to the serialization format.
	/// </summary>
	public override string ToString(object? value)
	{
		try
		{
			var bytesValue = MessagePackSerializer.Serialize(value, s_serializerOptions);
			return MessagePackSerializer.ConvertToJson(bytesValue);
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	public override byte[] ToBytes(object? value)
	{
		try
		{
			return MessagePackSerializer.Serialize(value, s_serializerOptions);
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Serializes a value to the serialization format.
	/// </summary>
	public override void ToStream(object? value, Stream outputStream)
	{
		try
		{
			MessagePackSerializer.Serialize(outputStream, value, s_serializerOptions);
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
	public override object? FromString(string stringValue, Type type)
	{
		try
		{
			var bytesValue = MessagePackSerializer.ConvertFromJson(stringValue);
			return MessagePackSerializer.Deserialize(type, bytesValue, s_serializerOptions);
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	public override object? FromBytes(byte[] bytesValue, Type type)
	{
		try
		{
			return MessagePackSerializer.Deserialize(type, bytesValue, s_serializerOptions);
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

	public override async Task<object?> FromStreamAsync(Stream stream, Type type, CancellationToken cancellationToken)
	{
		try
		{
			return await MessagePackSerializer.DeserializeAsync(type, stream, s_serializerOptions, cancellationToken).ConfigureAwait(false);
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	public override ServiceObject? ToServiceObject(object? value) => SystemTextJsonServiceSerializer.Instance.ToServiceObject(value);

	public override object? FromServiceObject(ServiceObject? serviceObject, Type type) => SystemTextJsonServiceSerializer.Instance.FromServiceObject(serviceObject, type);

	private static readonly MessagePackSerializerOptions s_serializerOptions = MessagePackSerializerOptions.Standard;
}
