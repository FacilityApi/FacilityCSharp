using Newtonsoft.Json;

namespace Facility.Core
{
	/// <summary>
	/// Used to JSON-serialize string-based enumerated types.
	/// </summary>
	public abstract class ServiceEnumJsonConverter<T> : ServiceJsonConverterBase<T>
	{
		protected abstract T CreateCore(string value);

		protected sealed override T ReadCore(JsonReader reader, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
				throw new JsonSerializationException($"Expected string for {typeof(T).Name}; got {reader.TokenType}.");

			return CreateCore((string) reader.Value);
		}

		protected sealed override void WriteCore(JsonWriter writer, T value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}
	}
}
