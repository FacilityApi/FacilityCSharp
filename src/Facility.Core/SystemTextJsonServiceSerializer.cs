using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values to and from JSON using <c>System.Text.Json</c>.
/// </summary>
public sealed class SystemTextJsonServiceSerializer : ServiceSerializer
{
	/// <summary>
	/// The serializer instance.
	/// </summary>
	public static readonly SystemTextJsonServiceSerializer Instance = new();

	public override string DefaultMediaType => "application/json";

	/// <summary>
	/// Serializes a value to the serialization format.
	/// </summary>
	public override string ToString(object? value)
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
	/// Serializes a value to the serialization format.
	/// </summary>
	public override void ToStream(object? value, Stream outputStream)
	{
		try
		{
			JsonSerializer.Serialize(outputStream, value, s_jsonSerializerOptions);
		}
		catch (JsonException exception)
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
			return JsonSerializer.Deserialize(stringValue, type, s_jsonSerializerOptions);
		}
		catch (JsonException exception)
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
			return JsonSerializer.Deserialize(stream, type, s_jsonSerializerOptions);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	/// <summary>
	/// Serializes a value to a <see cref="ServiceObject"/> representation of the serialization format.
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
	/// Deserializes a value from a <see cref="ServiceObject"/> representation of the serialization format.
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

	private static readonly JsonSerializerOptions s_jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};
}
