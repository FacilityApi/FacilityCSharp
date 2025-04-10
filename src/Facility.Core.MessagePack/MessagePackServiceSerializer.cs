using MessagePack;

namespace Facility.Core.MessagePack;

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
	public override string DefaultMediaType => "application/msgpack";

	/// <summary>
	/// Serializes a value to the serialization format.
	/// </summary>
	public override async Task ToStreamAsync(object? value, Stream stream, CancellationToken cancellationToken)
	{
		try
		{
			await MessagePackSerializer.SerializeAsync(stream, value, s_serializerOptions, cancellationToken).ConfigureAwait(false);
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
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

	public override T Clone<T>(T value)
	{
		if (value is null)
			return default!;

		try
		{
			return MessagePackSerializer.Deserialize<T>(MessagePackSerializer.Serialize(value, s_serializerOptions), s_serializerOptions);
		}
		catch (MessagePackSerializationException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Checks two values for equality by comparing serialized representations.
	/// </summary>
	public override bool AreEquivalent(object? value1, object? value2)
	{
		if (value1 is null || value2 is null)
			return value1 == value2;

		var type = value1.GetType();
		if (type != value2.GetType())
			return false;

		using var stream1 = new MemoryStream();
		MessagePackSerializer.Serialize(stream1, value1, s_serializerOptions);
		stream1.TryGetBuffer(out var buffer1);

		using var stream2 = ServiceDataEquivalenceStream.Create(buffer1.AsMemory());
		MessagePackSerializer.Serialize(stream2, value2, s_serializerOptions);
		return stream2.Equivalent;
	}

	private MessagePackServiceSerializer()
	{
	}

	private static readonly MessagePackSerializerOptions s_serializerOptions =
		MessagePackSerializerOptions.Standard.WithResolver(MessagePackServiceResolver.Instance);
}
