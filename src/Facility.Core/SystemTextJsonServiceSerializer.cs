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
		JsonSerializer.Serialize(stream1, value1, s_jsonSerializerOptions);
		stream1.TryGetBuffer(out var buffer1);

		using var stream2 = ServiceDataEquivalenceStream.Create(buffer1.AsMemory());
		JsonSerializer.Serialize(stream2, value2, s_jsonSerializerOptions);
		return stream2.Equivalent;
	}

	/// <summary>
	/// Creates and configures JSON serializer options.
	/// </summary>
	public static JsonSerializerOptions CreateJsonSerializerOptions()
	{
		var options = new JsonSerializerOptions();
		ConfigureJsonSerializerOptions(options);
		return options;
	}

	/// <summary>
	/// Configures the JSON serializer options to match what Facility uses.
	/// </summary>
	public static void ConfigureJsonSerializerOptions(JsonSerializerOptions options)
	{
		options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
		options.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals;
		options.PropertyNameCaseInsensitive = true;
		options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
		options.Converters.Add(new DateTimeConverter());
		options.Converters.Add(new AllowReadingStringFromNonStringConverter());
		options.Converters.Add(new AllowReadingBooleanFromStringConverter());
		options.Converters.Add(new NewtonsoftJsonLinqSystemTextJsonConverter<JObject>());
		options.Converters.Add(new NewtonsoftJsonLinqSystemTextJsonConverter<JArray>());
		options.Converters.Add(new NewtonsoftJsonLinqSystemTextJsonConverter<JValue>());
		options.Converters.Add(new NewtonsoftJsonLinqSystemTextJsonConverter<JToken>());
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

		public override string ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.GetString()!;

		public override void WriteAsPropertyName(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
			writer.WritePropertyName(value);
	}

	private sealed class DateTimeConverter : JsonConverter<DateTime>
	{
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			ServiceDataUtility.TryParseDateTime(reader.GetString()) ?? throw new JsonException();

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			try
			{
				writer.WriteStringValue(ServiceDataUtility.RenderDateTime(value));
			}
			catch (ArgumentException exception)
			{
				throw new JsonException(null, exception);
			}
		}
	}

	private static readonly JsonSerializerOptions s_jsonSerializerOptions = CreateJsonSerializerOptions();
}
