using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values to and from JSON using <c>System.Text.Json</c>.
/// </summary>
public class SystemTextJsonContextServiceSerializer : JsonServiceSerializer
{
	public SystemTextJsonContextServiceSerializer(JsonSerializerContext serializerContext)
	{
		m_serializerContext = serializerContext;
	}

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public override string ToJson(object? value)
	{
		if (value is not ServiceDto)
			return SystemTextJsonServiceSerializer.Instance.ToJson(value);

		try
		{
			return JsonSerializer.Serialize(value, value.GetType(), m_serializerContext);
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
		if (!typeof(ServiceDto).IsAssignableFrom(type))
			return SystemTextJsonServiceSerializer.Instance.FromJson(json, type);

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
		if (value is not ServiceDto)
			return SystemTextJsonServiceSerializer.Instance.ToServiceObject(value);

		try
		{
			return ServiceObject.Create((JsonObject) JsonSerializer.SerializeToNode(value, value.GetType(), m_serializerContext)!);
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
		if (!typeof(ServiceDto).IsAssignableFrom(type))
			return SystemTextJsonServiceSerializer.Instance.FromServiceObject(serviceObject, type);

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
		if (value is not ServiceDto)
		{
			await SystemTextJsonServiceSerializer.Instance.ToStreamAsync(value, stream, cancellationToken).ConfigureAwait(false);
			return;
		}

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
		if (!typeof(ServiceDto).IsAssignableFrom(type))
			return await SystemTextJsonServiceSerializer.Instance.FromStreamAsync(stream, type, cancellationToken).ConfigureAwait(false);

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

		if (value is not ServiceDto)
			return SystemTextJsonServiceSerializer.Instance.Clone(value);

		using var memoryStream = new MemoryStream();
		JsonSerializer.Serialize(memoryStream, value, typeof(T), m_serializerContext);
		memoryStream.Position = 0;
		return (T) JsonSerializer.Deserialize(memoryStream, typeof(T), m_serializerContext)!;
	}

	private readonly JsonSerializerContext m_serializerContext;
}
