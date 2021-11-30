using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facility.Core;

[JsonConverter(typeof(ServiceObjectNewtonsoftJsonConverter))]
public sealed class ServiceObject
{
	[return: NotNullIfNotNull("jObject")]
	public static ServiceObject? Create(JObject? jObject) => jObject is null ? null : new(jObject);

	public JObject AsJObject() => m_jObject;

	public bool IsEquivalentTo(ServiceObject? other) => other is not null && JToken.DeepEquals(AsJObject(), other.AsJObject());

	private ServiceObject(JObject jObject)
	{
		m_jObject = jObject;
	}

	private readonly JObject m_jObject;
}
