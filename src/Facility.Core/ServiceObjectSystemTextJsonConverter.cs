using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Facility.Core;

/// <summary>
/// Used by <c>System.Text.Json</c> to convert <see cref="ServiceObject" />.
/// </summary>
public sealed class ServiceObjectSystemTextJsonConverter : JsonConverter<ServiceObject>
{
	/// <summary>
	/// Reads the JSON representation of the value.
	/// </summary>
	public override ServiceObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var jsonNode = JsonNode.Parse(ref reader);
		if (jsonNode is not JsonObject)
			throw new JsonException("Expected object for ServiceObject.");

		return ServiceObject.Create(jsonNode.AsObject());
	}

	/// <summary>
	/// Writes the JSON representation of the value.
	/// </summary>
	public override void Write(Utf8JsonWriter writer, ServiceObject value, JsonSerializerOptions options) =>
		value.AsJsonObject(ServiceObjectAccess.ReadOnly).WriteTo(writer, options);
}
