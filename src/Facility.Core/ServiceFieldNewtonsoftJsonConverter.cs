using Newtonsoft.Json;

namespace Facility.Core;

/// <summary>
/// Used by Json.NET to convert <see cref="ServiceObject" />.
/// </summary>
public sealed class ServiceFieldNewtonsoftJsonConverter : JsonConverter
{
	/// <summary>
	/// Implements CanConvert.
	/// </summary>
	public override bool CanConvert(Type objectType) =>
		objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ServiceField<>);

	/// <summary>
	/// Implements ReadJson.
	/// </summary>
	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		var valueType = objectType.GetGenericArguments().Single();
		var value = serializer.Deserialize(reader, valueType);
		return objectType.GetConstructor(new[] { valueType })!.Invoke(new[] { value });
	}

	/// <summary>
	/// Implements WriteJson.
	/// </summary>
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		var optional = (IServiceField) value!;
		if (optional.IsDefault)
		{
			throw new InvalidOperationException("Service field must not be default. " +
				"Properties should include these attributes: " +
				"[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, " +
				"NullValueHandling = NullValueHandling.Include), " +
				"ServiceFieldDefaultValue(typeof(ServiceField<...>))]");
		}

		serializer.Serialize(writer, optional.Value);
	}
}
