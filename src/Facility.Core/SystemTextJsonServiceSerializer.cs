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
	public override async Task ToStreamAsync(object? value, Stream stream, CancellationToken cancellationToken)
	{
		try
		{
			await JsonSerializer.SerializeAsync(stream, value, s_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
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
			return await JsonSerializer.DeserializeAsync(stream, type, s_jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
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
		JsonSerializer.Serialize(memoryStream, value, s_jsonSerializerOptions);
		memoryStream.Position = 0;
		return JsonSerializer.Deserialize<T>(memoryStream, s_jsonSerializerOptions)!;
	}

	private SystemTextJsonServiceSerializer()
	{
	}

	private sealed class NewtonsoftJsonLinqSystemTextJsonConverter<T> : JsonConverter<T>
		where T : Newtonsoft.Json.Linq.JToken
	{
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			(T) Newtonsoft.Json.Linq.JToken.Parse(JsonNode.Parse(ref reader)?.ToJsonString());

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
			writer.WriteRawValue(value.ToString(Newtonsoft.Json.Formatting.None));
	}

	private static readonly JsonSerializerOptions s_jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Converters =
		{
			new NewtonsoftJsonLinqSystemTextJsonConverter<Newtonsoft.Json.Linq.JObject>(),
			new NewtonsoftJsonLinqSystemTextJsonConverter<Newtonsoft.Json.Linq.JArray>(),
			new NewtonsoftJsonLinqSystemTextJsonConverter<Newtonsoft.Json.Linq.JValue>(),
			new NewtonsoftJsonLinqSystemTextJsonConverter<Newtonsoft.Json.Linq.JToken>(),
		},
	};
}
