using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.Core;

public class ServiceObjectNewtonsoftJsonConverter : ServiceJsonConverterBase<ServiceObject>
{
	protected override ServiceObject ReadCore(JsonReader reader, JsonSerializer serializer)
	{
		return ServiceObject.Create((JObject) JToken.ReadFrom(reader))!;
	}

	protected override void WriteCore(JsonWriter writer, ServiceObject value, JsonSerializer serializer)
	{
		var jObject = value.AsJObject();
		if (jObject is null)
			writer.WriteNull();
		else
			jObject.WriteTo(writer, serializer.Converters.ToArray());
	}
}
