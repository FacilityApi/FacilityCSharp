using System.Text.Json;
using System.Text.Json.Nodes;

namespace Facility.Core;

public class ServiceObjectSystemTextJsonConverter : ServiceSystemTextJsonConverterBase<ServiceObject>
{
	protected override ServiceObject ReadCore(ref Utf8JsonReader reader, JsonSerializerOptions options)
	{
		var jsonObject = JsonNode.Parse(ref reader)!.AsObject();
		return ServiceObject.Create(jsonObject)!;
	}

	public override void Write(Utf8JsonWriter writer, ServiceObject value, JsonSerializerOptions options)
	{
		var jsonObject = value.AsJsonObject();
		if (jsonObject is null)
			writer.WriteNullValue();
		else
			jsonObject.WriteTo(writer, options);
	}
}
