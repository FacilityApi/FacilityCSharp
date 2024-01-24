using Newtonsoft.Json;

namespace Facility.Core;

/// <summary>
/// Used by Json.NET to convert <see cref="ServiceNullable{T}" />.
/// </summary>
public sealed class ServiceNullableNewtonsoftJsonConverter : JsonConverter
{
	/// <summary>
	/// Implements CanConvert.
	/// </summary>
	public override bool CanConvert(Type objectType) =>
		objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ServiceNullable<>);

	/// <summary>
	/// Implements ReadJson.
	/// </summary>
	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		var valueType = objectType.GetGenericArguments().Single();
		var value = serializer.Deserialize(reader, valueType);
		return objectType.GetConstructor([valueType])!.Invoke([value]);
	}

	/// <summary>
	/// Implements WriteJson.
	/// </summary>
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		var nullable = (IServiceNullable) value!;
		if (nullable.IsUnspecified)
		{
			throw new InvalidOperationException("ServiceNullable must not be default. " +
				"Properties should include these attributes: " +
				"[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, " +
				"NullValueHandling = NullValueHandling.Include), " +
				"ServiceNullableDefaultValue(typeof(ServiceNullable<...>))]");
		}

		serializer.Serialize(writer, nullable.Value);
	}
}
