using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Facility.Core;

/// <summary>
/// Used by <c>System.Text.Json</c> to convert <see cref="ServiceField{T}" />.
/// </summary>
public sealed class ServiceFieldSystemTextJsonConverter : JsonConverterFactory
{
	/// <summary>
	/// Determines whether this instance can convert the specified object type.
	/// </summary>
	public override bool CanConvert(Type typeToConvert) =>
		typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ServiceField<>);

	/// <summary>
	/// Creates a converter for the specified object type.
	/// </summary>
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var elementType = typeToConvert.GetGenericArguments()[0];
		return (JsonConverter) Activator.CreateInstance(
			typeof(ServiceFieldSystemTextJsonConverter<>).MakeGenericType(elementType),
			BindingFlags.Instance | BindingFlags.Public, binder: null, args: null, culture: null)!;
	}
}

/// <summary>
/// Used by <c>System.Text.Json</c> to convert <see cref="ServiceField{T}" />.
/// </summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Same name.")]
public sealed class ServiceFieldSystemTextJsonConverter<T> : JsonConverter<ServiceField<T>>
{
	/// <summary>
	/// Reads the JSON representation of the object.
	/// </summary>
	public override ServiceField<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		reader.TokenType is JsonTokenType.Null ? new ServiceField<T>(default!) : JsonSerializer.Deserialize<T>(ref reader, options)!;

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	public override void Write(Utf8JsonWriter writer, ServiceField<T> value, JsonSerializerOptions options)
	{
		if (value.IsDefault)
		{
			throw new InvalidOperationException("Service field must not be default. " +
				"Properties should include this attribute: " +
				"[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]");
		}

		if (value.Value is { } fieldValue)
			JsonSerializer.Serialize<T>(writer, fieldValue, options);
		else
			writer.WriteNullValue();
	}
}
