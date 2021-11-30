using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace Facility.Core;

[Newtonsoft.Json.JsonConverter(typeof(ServiceObjectNewtonsoftJsonConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(ServiceObjectSystemTextJsonConverter))]
public sealed class ServiceObject
{
	[return: NotNullIfNotNull("jObject")]
	public static ServiceObject? Create(JObject? jObject) => jObject is null ? null : new(jObject);

	[return: NotNullIfNotNull("jsonObject")]
	public static ServiceObject? Create(JsonObject? jsonObject) => jsonObject is null ? null : new(jsonObject);

	public JObject AsJObject() => m_jObject ?? ServiceSerializer.Default.FromString<JObject>(ToJsonString())!;

	public JsonObject AsJsonObject() => m_jsonObject ?? SystemTextJsonServiceSerializer.Instance.FromString<JsonObject>(ToJsonString())!;

	public bool IsEquivalentTo(ServiceObject? other) => other is not null &&
		(
			m_jObject is { } jObject ? JToken.DeepEquals(jObject, other.AsJObject()) :
			m_jsonObject is { } jsonObject ? SystemTextJsonUtility.DeepEquals(jsonObject, other.AsJsonObject()) :
			throw new InvalidOperationException()
		);

	private ServiceObject(JObject jObject)
	{
		m_jObject = jObject;
	}

	private ServiceObject(JsonObject jsonObject)
	{
		m_jsonObject = jsonObject;
	}

	private string ToJsonString() =>
		m_jObject is { } jObject ? ServiceSerializer.Default.ToString(jObject) :
		m_jsonObject is { } jsonObject ? SystemTextJsonServiceSerializer.Instance.ToString(jsonObject) :
		throw new InvalidOperationException();

	private readonly JObject? m_jObject;
	private readonly JsonObject? m_jsonObject;
}
