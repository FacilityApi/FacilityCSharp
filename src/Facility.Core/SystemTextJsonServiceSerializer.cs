using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Facility.Core;

public sealed class SystemTextJsonServiceSerializer : ServiceSerializer
{
	public static readonly SystemTextJsonServiceSerializer Instance = new();

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
