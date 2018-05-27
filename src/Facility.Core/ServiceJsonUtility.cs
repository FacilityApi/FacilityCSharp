using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Facility.Core
{
	/// <summary>
	/// Helper methods for working with JSON.
	/// </summary>
	public static class ServiceJsonUtility
	{
		/// <summary>
		/// Serializes a value to JSON.
		/// </summary>
		public static string ToJson(object value)
		{
			using (StringWriter stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture))
			{
				ToJsonTextWriter(value, stringWriter);
				return stringWriter.ToString();
			}
		}

		/// <summary>
		/// Serializes a value to JSON.
		/// </summary>
		public static void ToJsonStream(object value, Stream outputStream)
		{
			// don't dispose the StreamWriter to avoid closing the stream
			StreamWriter textWriter = new StreamWriter(outputStream);
			ToJsonTextWriter(value, textWriter);
			textWriter.Flush();
		}

		/// <summary>
		/// Serializes a value to JSON.
		/// </summary>
		public static void ToJsonTextWriter(object value, TextWriter textWriter)
		{
			using (JsonTextWriter jsonTextWriter = new JsonTextWriter(textWriter) { Formatting = Formatting.None, CloseOutput = false })
				ToJsonWriter(value, jsonTextWriter);
		}

		/// <summary>
		/// Serializes a value to JSON.
		/// </summary>
		public static void ToJsonWriter(object value, JsonWriter jsonWriter)
		{
			JsonSerializer.Create(s_jsonSerializerSettings).Serialize(jsonWriter, value);
		}

		/// <summary>
		/// Deserializes a value from JSON.
		/// </summary>
		public static T FromJson<T>(string json)
		{
			return (T) FromJson(json, typeof(T));
		}

		/// <summary>
		/// Deserializes a value from JSON.
		/// </summary>
		public static object FromJson(string json, Type type)
		{
			using (StringReader stringReader = new StringReader(json))
				return FromJsonTextReader(stringReader, type);
		}

		/// <summary>
		/// Deserializes a value from JSON.
		/// </summary>
		public static T FromJsonTextReader<T>(TextReader textReader)
		{
			return (T) FromJsonTextReader(textReader, typeof(T));
		}

		/// <summary>
		/// Deserializes a value from JSON.
		/// </summary>
		public static object FromJsonTextReader(TextReader textReader, Type type)
		{
			using (JsonReader reader = new JsonTextReader(textReader))
				return FromJsonReader(reader, type);
		}

		/// <summary>
		/// Deserializes a value from JSON.
		/// </summary>
		public static T FromJsonReader<T>(JsonReader reader)
		{
			return (T) FromJsonReader(reader, typeof(T));
		}

		/// <summary>
		/// Deserializes a value from JSON.
		/// </summary>
		public static object FromJsonReader(JsonReader reader, Type type)
		{
			JsonSerializer serializer = JsonSerializer.Create(s_jsonSerializerSettings);
			object value = serializer.Deserialize(reader, type);
			if (reader.Read() && reader.TokenType != JsonToken.Comment)
				throw new JsonSerializationException("Additional text found in JSON after deserializing.");
			if (value == null && type == typeof(JToken))
				value = JValue.CreateNull();
			return value;
		}

		/// <summary>
		/// Deserializes a value from a JToken.
		/// </summary>
		public static T FromJToken<T>(JToken jToken)
		{
			return (T) FromJToken(jToken, typeof(T));
		}

		/// <summary>
		/// Deserializes a value from a JToken.
		/// </summary>
		public static object FromJToken(JToken jToken, Type type)
		{
			if (jToken == null || JToken.DeepEquals(jToken, null))
				return null;

			using (JsonReader reader = new JTokenReader(jToken))
				return FromJsonReader(reader, type);
		}

		/// <summary>
		/// Converts the object to a JToken.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The JToken.</returns>
		public static JToken ToJToken(object value)
		{
			// use JSON to avoid unusual types like byte arrays in the JToken
			return FromJson<JToken>(ToJson(value));
		}

		/// <summary>
		/// Clones a JToken.
		/// </summary>
		public static T CloneToken<T>(T token) where T : JToken
		{
			return (T) token?.DeepClone();
		}

		/// <summary>
		/// Creates a JSON serializer with standard settings.
		/// </summary>
		public static JsonSerializer CreateJsonSerializer()
		{
			return JsonSerializer.Create(s_jsonSerializerSettings);
		}

		/// <summary>
		/// Gets the standard JSON serializer settings.
		/// </summary>
		public static JsonSerializerSettings GetJsonSerializerSettings()
		{
			var settings = new JsonSerializerSettings();
			ApplyJsonSerializerSettings(settings);
			return settings;
		}

		/// <summary>
		/// Applies the standard JSON serializer settings.
		/// </summary>
		public static void ApplyJsonSerializerSettings(JsonSerializerSettings settings)
		{
			settings.ContractResolver = new CamelCaseExceptDictionaryKeysContractResolver();
			settings.DateParseHandling = DateParseHandling.None;
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.MissingMemberHandling = MissingMemberHandling.Ignore;
			settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
		}

		private sealed class CamelCaseExceptDictionaryKeysContractResolver : CamelCasePropertyNamesContractResolver
		{
			protected override string ResolveDictionaryKey(string dictionaryKey)
			{
				return dictionaryKey;
			}
		}

		static readonly JsonSerializerSettings s_jsonSerializerSettings = GetJsonSerializerSettings();
	}
}
