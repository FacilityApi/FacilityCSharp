using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values to and from JSON using a <c>System.Text.Json</c>
/// serializer context.
/// </summary>
/// <remarks>Only the types supported by the serializer context can be serialized
/// or deserialized.</remarks>
public class SystemTextJsonContextServiceSerializer : JsonServiceSerializer
{
	/// <summary>
	/// Creates an instance that uses the specified serializer context.
	/// </summary>
	public SystemTextJsonContextServiceSerializer(JsonSerializerContext serializerContext)
	{
		m_serializerContext = serializerContext;
	}

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public override string ToJson(object? value)
	{
		try
		{
			return JsonSerializer.Serialize(value, value!.GetType(), m_serializerContext);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
	public override object? FromJson(string json, Type type)
	{
		try
		{
			return JsonSerializer.Deserialize(json, type, m_serializerContext);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
	public override T? FromJson<T>(string json)
		where T : default
	{
		return (T?) FromJson(json, typeof(T));
	}

	/// <summary>
	/// Serializes a value to a <see cref="ServiceObject"/> representation of JSON.
	/// </summary>
	public override ServiceObject? ToServiceObject(object? value)
	{
		try
		{
			return ServiceObject.Create((JsonObject) JsonSerializer.SerializeToNode(value, value!.GetType(), m_serializerContext)!);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Deserializes a value from a <see cref="ServiceObject"/> representation of JSON.
	/// </summary>
	public override object? FromServiceObject(ServiceObject? serviceObject, Type type)
	{
		try
		{
			return serviceObject?.AsJsonObject().Deserialize(type, m_serializerContext);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Deserializes a value from a <see cref="ServiceObject"/> representation of JSON.
	/// </summary>
	public override T? FromServiceObject<T>(ServiceObject? serviceObject)
		where T : default
	{
		return (T?) FromServiceObject(serviceObject, typeof(T));
	}

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public override async Task ToStreamAsync(object? value, Stream stream, CancellationToken cancellationToken)
	{
		try
		{
			await JsonSerializer.SerializeAsync(stream, value, value!.GetType(), m_serializerContext, cancellationToken).ConfigureAwait(false);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
	public override async Task<object?> FromStreamAsync(Stream stream, Type type, CancellationToken cancellationToken)
	{
		try
		{
			return await JsonSerializer.DeserializeAsync(stream, type, m_serializerContext, cancellationToken).ConfigureAwait(false);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Clones a value by serializing and deserializing.
	/// </summary>
	public override T Clone<T>(T value)
	{
		if (value is null)
			return default!;

		using var memoryStream = new MemoryStream();
		JsonSerializer.Serialize(memoryStream, value, typeof(T), m_serializerContext);
		memoryStream.Position = 0;
		return (T) JsonSerializer.Deserialize(memoryStream, typeof(T), m_serializerContext)!;
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
		JsonSerializer.Serialize(stream1, value1, type, m_serializerContext);
		stream1.TryGetBuffer(out var buffer1);

		using var stream2 = ServiceDataEquivalenceStream.Create(buffer1.AsMemory());
		JsonSerializer.Serialize(stream2, value2, type, m_serializerContext);
		return stream2.Equivalent;
	}

	private readonly JsonSerializerContext m_serializerContext;
}
