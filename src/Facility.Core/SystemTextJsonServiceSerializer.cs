using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

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
	/// Serializes a value to JSON.
	/// </summary>
	public void ToStream(object? value, Stream stream)
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
	public object? FromStream(Stream stream, Type type)
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
		where T : JToken
	{
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			ServiceJsonUtility.FromJson<T>(JsonNode.Parse(ref reader)!.ToJsonString(s_jsonSerializerOptions));

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
			writer.WriteRawValue(ServiceJsonUtility.ToJson(value));
	}

	private sealed class AllowReadingBooleanFromStringConverter : JsonConverter<bool>
	{
		public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType switch
			{
				JsonTokenType.True => true,
				JsonTokenType.False => false,
				JsonTokenType.String when reader.ValueTextEquals("true"u8) => true,
				JsonTokenType.String when reader.ValueTextEquals("false"u8) => false,
				_ => throw new JsonException(),
			};

		public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
			writer.WriteBooleanValue(value);
	}

	private sealed class AllowReadingStringFromNonStringConverter : JsonConverter<string>
	{
		public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType switch
			{
				JsonTokenType.String => reader.GetString()!,
				JsonTokenType.True => "true",
				JsonTokenType.False => "false",
#if !NETSTANDARD2_0
				JsonTokenType.Number => Encoding.UTF8.GetString(reader.ValueSpan),
#else
				JsonTokenType.Number => Encoding.UTF8.GetString(reader.ValueSpan.ToArray()),
#endif
				_ => throw new JsonException(),
			};

		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
			writer.WriteStringValue(value);
	}

	private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters =
		{
			new AllowReadingStringFromNonStringConverter(),
			new AllowReadingBooleanFromStringConverter(),
			new NewtonsoftJsonLinqSystemTextJsonConverter<JObject>(),
			new NewtonsoftJsonLinqSystemTextJsonConverter<JArray>(),
			new NewtonsoftJsonLinqSystemTextJsonConverter<JValue>(),
			new NewtonsoftJsonLinqSystemTextJsonConverter<JToken>(),
		},
	};
}
