using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Facility.Core;

/// <summary>
/// Serializes to and from JSON using <see cref="JsonSerializer"/>.
/// </summary>
public sealed class NewtonsoftJsonServiceSerializer : ServiceSerializer
{
	public static readonly NewtonsoftJsonServiceSerializer Instance = new();

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public override string ToString(object? value)
	{
		using var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
		ToJsonTextWriter(value, stringWriter);
		return stringWriter.ToString();
	}

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	public override void ToStream(object? value, Stream outputStream)
	{
		// don't dispose the StreamWriter to avoid closing the stream
		var textWriter = new StreamWriter(outputStream);
		ToJsonTextWriter(value, textWriter);
		textWriter.Flush();
	}

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
	private static void ToJsonTextWriter(object? value, TextWriter textWriter)
	{
		using var jsonTextWriter = new JsonTextWriter(textWriter) { Formatting = Formatting.None, CloseOutput = false };
		ToJsonWriter(value, jsonTextWriter);
	}

	/// <summary>
	/// Serializes a value to JSON.
	/// </summary>
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

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
	public override object? FromString(string stringValue, Type type)
	{
		using var stringReader = new StringReader(stringValue);
		return FromJsonTextReader(stringReader, type);
	}

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
	private object? FromJsonTextReader(TextReader textReader, Type type)
	{
		using var reader = new JsonTextReader(textReader);
		return FromJsonReader(reader, type);
	}

	/// <summary>
	/// Deserializes a value from JSON.
	/// </summary>
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

	public override object? FromStream(Stream stream, Type type)
	{
		using var textReader = new StreamReader(stream);
		return FromJsonTextReader(textReader, type);
	}

	// use JSON to avoid unusual types like byte arrays in the JToken
	public override ServiceObject? ToServiceObject(object? value) => value is null ? null : ServiceObject.Create(FromString<JObject>(ToString(value)));

	public override object? FromServiceObject(ServiceObject? serviceObject, Type type) => serviceObject?.AsJObject().ToObject(type, CreateJsonSerializer());

	/// <summary>
	/// Creates a JSON serializer with standard settings.
	/// </summary>
	public static JsonSerializer CreateJsonSerializer() => JsonSerializer.Create(s_jsonSerializerSettings);

	/// <summary>
	/// Gets the standard JSON serializer settings.
	/// </summary>
	private static JsonSerializerSettings GetJsonSerializerSettings()
	{
		var settings = new JsonSerializerSettings();
		ApplyJsonSerializerSettings(settings);
		return settings;
	}

	/// <summary>
	/// Applies the standard JSON serializer settings.
	/// </summary>
	private static void ApplyJsonSerializerSettings(JsonSerializerSettings settings)
	{
		settings.ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver();
		settings.DateParseHandling = DateParseHandling.None;
		settings.NullValueHandling = NullValueHandling.Ignore;
		settings.MissingMemberHandling = MissingMemberHandling.Ignore;
		settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
	}

	private sealed class CamelCaseExceptDictionaryKeysContractResolver : CamelCasePropertyNamesContractResolver
	{
		protected override string ResolveDictionaryKey(string dictionaryKey) => dictionaryKey;
	}

	private static readonly JsonSerializerSettings s_jsonSerializerSettings = GetJsonSerializerSettings();
}
