using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values to and from JSON using <c>System.Text.Json</c>.
/// </summary>
public sealed class SystemTextJsonServiceSerializer : JsonServiceSerializer
{
	/// <summary>
	/// The serializer instance.
	/// </summary>
	public static readonly SystemTextJsonServiceSerializer Instance = new();

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public override string ToJson(object? value)
	{
		try
		{
			return JsonSerializer.Serialize(value, s_jsonSerializerOptions);
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
			return JsonSerializer.Deserialize(json, type, s_jsonSerializerOptions);
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
		try
		{
			return JsonSerializer.Deserialize<T>(json, s_jsonSerializerOptions);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Serializes a value to a <see cref="ServiceObject"/> representation of JSON.
	/// </summary>
	public override ServiceObject? ToServiceObject(object? value)
	{
		try
		{
			return value is null ? null : ServiceObject.Create((JsonObject) JsonSerializer.SerializeToNode(value, s_jsonSerializerOptions)!);
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
			return serviceObject?.AsJsonObject().Deserialize(type, s_jsonSerializerOptions);
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
		try
		{
			return serviceObject is null ? default : serviceObject.AsJsonObject().Deserialize<T>(s_jsonSerializerOptions);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public override void ToStream(object? value, Stream stream)
	{
		try
		{
			JsonSerializer.Serialize(stream, value, s_jsonSerializerOptions);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
	public override object? FromStream(Stream stream, Type type)
	{
		try
		{
			return JsonSerializer.Deserialize(stream, type, s_jsonSerializerOptions);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	private SystemTextJsonServiceSerializer()
	{
	}

	private static readonly JsonSerializerOptions s_jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};
}
