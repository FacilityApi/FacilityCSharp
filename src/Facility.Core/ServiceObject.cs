using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.Core;

[JsonConverter(typeof(ServiceObjectJsonConverter))]
public sealed class ServiceObject
{
	public ServiceObject(JObject? jObject)
	{
		m_jObject = jObject;
	}

	public JObject? AsJObject() => m_jObject;

	public bool IsEquivalentTo(ServiceObject? other) => JToken.DeepEquals(AsJObject(), other?.AsJObject());

	private readonly JObject? m_jObject;
}

public class ServiceObjectJsonConverter : ServiceJsonConverterBase<ServiceObject>
{
	protected override ServiceObject ReadCore(JsonReader reader, JsonSerializer serializer)
	{
		return new ServiceObject((JObject) JToken.ReadFrom(reader));
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
