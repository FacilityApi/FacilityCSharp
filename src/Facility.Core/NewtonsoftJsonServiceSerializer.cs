using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Facility.Core;

/// <summary>
/// Serializes to and from JSON using <see cref="JsonSerializer"/>.
/// </summary>
public sealed class NewtonsoftJsonServiceSerializer : JsonServiceSerializer
{
	/// <summary>
	/// The serializer instance.
	/// </summary>
	public static readonly NewtonsoftJsonServiceSerializer Instance = new();

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public override string ToJson(object? value)
	{
		using var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
		ToJsonTextWriter(value, stringWriter);
		return stringWriter.ToString();
	}

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
	public override object? FromJson(string json, Type type)
	{
		using var stringReader = new StringReader(json);
		return FromJsonTextReader(stringReader, type);
	}

	/// <summary>
	/// Serializes a value to a <see cref="ServiceObject"/> representation of the serialization format.
	/// </summary>
	public override ServiceObject? ToServiceObject(object? value)
	{
		// use JSON to avoid unusual types like byte arrays in the JToken
		return value is null ? null : ServiceObject.Create(FromJson<JObject>(ToJson(value)));
	}

	/// <summary>
	/// Deserializes a value from a <see cref="ServiceObject"/> representation of the serialization format.
	/// </summary>
	public override object? FromServiceObject(ServiceObject? serviceObject, Type type) =>
		serviceObject?.AsJObject().ToObject(type, CreateJsonSerializer());

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public override void ToStream(object? value, Stream stream)
	{
		// don't dispose the StreamWriter to avoid closing the stream
		var textWriter = new StreamWriter(stream);
		ToJsonTextWriter(value, textWriter);
		textWriter.Flush();
	}

	/// <summary>
	/// Deserializes a value from the serialization format.
	/// </summary>
	public override object? FromStream(Stream stream, Type type)
	{
		using var textReader = new StreamReader(stream);
		return FromJsonTextReader(textReader, type);
	}

	/// <summary>
	/// Creates a JSON serializer with standard settings.
	/// </summary>
	public static JsonSerializer CreateJsonSerializer() => JsonSerializer.Create(s_jsonSerializerSettings);

	private static void ToJsonTextWriter(object? value, TextWriter textWriter)
	{
		using var jsonTextWriter = new JsonTextWriter(textWriter) { Formatting = Formatting.None, CloseOutput = false };
		ToJsonWriter(value, jsonTextWriter);
	}

	private static void ToJsonWriter(object? value, JsonWriter jsonWriter)
	{
		try
		{
			JsonSerializer.Create(s_jsonSerializerSettings).Serialize(jsonWriter, value);
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	private object? FromJsonTextReader(TextReader textReader, Type type)
	{
		using var reader = new JsonTextReader(textReader);
		return FromJsonReader(reader, type);
	}

	private static object? FromJsonReader(JsonReader reader, Type type)
	{
		try
		{
			var serializer = JsonSerializer.Create(s_jsonSerializerSettings);
			var value = serializer.Deserialize(reader, type);
			if (reader.Read() && reader.TokenType != JsonToken.Comment)
				throw new ServiceSerializationException("Additional text found in JSON after deserializing.");
			if (value is null && type == typeof(JToken))
				value = JValue.CreateNull();
			return value;
		}
		catch (JsonException exception)
		{
			throw new ServiceSerializationException(exception);
		}
	}

	private NewtonsoftJsonServiceSerializer()
	{
	}

	private sealed class CamelCaseExceptDictionaryKeysContractResolver : CamelCasePropertyNamesContractResolver
	{
		protected override string ResolveDictionaryKey(string dictionaryKey) => dictionaryKey;
	}

	private static readonly JsonSerializerSettings s_jsonSerializerSettings = new()
	{
		ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver(),
		DateParseHandling = DateParseHandling.None,
		NullValueHandling = NullValueHandling.Ignore,
		MissingMemberHandling = MissingMemberHandling.Ignore,
		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
	};
}
