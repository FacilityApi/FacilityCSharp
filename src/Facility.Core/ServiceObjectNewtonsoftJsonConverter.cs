using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.Core;

/// <summary>
/// Used by Json.NET to convert <see cref="ServiceObject" />.
/// </summary>
public sealed class ServiceObjectNewtonsoftJsonConverter : ServiceJsonConverterBase<ServiceObject>
{
	/// <summary>
	/// Reads the JSON representation of the value.
	/// </summary>
	protected override ServiceObject ReadCore(JsonReader reader, JsonSerializer serializer) =>
		ServiceObject.Create((JObject) JToken.ReadFrom(reader));

	/// <summary>
	/// Writes the JSON representation of the value.
	/// </summary>
	protected override void WriteCore(JsonWriter writer, ServiceObject value, JsonSerializer serializer) =>
		value.AsJObject(ServiceObjectAccess.ReadOnly).WriteTo(writer, serializer.Converters.ToArray());
}
