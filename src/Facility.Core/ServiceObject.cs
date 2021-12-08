using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace Facility.Core;

/// <summary>
/// Encapsulates a JSON object.
/// </summary>
[Newtonsoft.Json.JsonConverter(typeof(ServiceObjectNewtonsoftJsonConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(ServiceObjectSystemTextJsonConverter))]
[MessagePack.MessagePackFormatter(typeof(ServiceObjectMessagePackFormatter))]
public sealed class ServiceObject
{
	/// <summary>
	/// Creates an instance from a <c>Newtonsoft.Json.Linq.JObject</c>.
	/// </summary>
	[return: NotNullIfNotNull("jObject")]
	public static ServiceObject? Create(JObject? jObject) => jObject is null ? null : new(jObject);

	/// <summary>
	/// Creates an instance from a <c>System.Text.Json.Nodes.JsonObject</c>.
	/// </summary>
	[return: NotNullIfNotNull("jsonObject")]
	public static ServiceObject? Create(JsonObject? jsonObject) => jsonObject is null ? null : new(jsonObject);

	/// <summary>
	/// Returns the JSON object as a <c>Newtonsoft.Json.Linq.JObject</c>.
	/// </summary>
	public JObject AsJObject() => m_jObject ?? NewtonsoftJsonServiceSerializer.Instance.FromJson<JObject>(ToString())!;

	/// <summary>
	/// Returns the JSON object as a <c>System.Text.Json.Nodes.JsonObject</c>.
	/// </summary>
	public JsonObject AsJsonObject() => m_jsonObject ?? SystemTextJsonServiceSerializer.Instance.FromJson<JsonObject>(ToString())!;

	/// <summary>
	/// Returns true if the JSON objects are equivalent.
	/// </summary>
	public bool IsEquivalentTo(ServiceObject? other) => other is not null &&
	(
		m_jObject is { } jObject ? JToken.DeepEquals(jObject, other.AsJObject()) :
		m_jsonObject is { } jsonObject ? SystemTextJsonUtility.DeepEquals(jsonObject, other.AsJsonObject()) :
		throw new InvalidOperationException()
	);

	/// <summary>
	/// Returns a JSON string for the JSON object.
	/// </summary>
	public override string ToString() =>
		m_jObject is { } jObject ? NewtonsoftJsonServiceSerializer.Instance.ToJson(jObject) :
		m_jsonObject is { } jsonObject ? SystemTextJsonServiceSerializer.Instance.ToJson(jsonObject) :
		"";

	private ServiceObject(JObject jObject) => m_jObject = jObject;

	private ServiceObject(JsonObject jsonObject) => m_jsonObject = jsonObject;

	private readonly JObject? m_jObject;
	private readonly JsonObject? m_jsonObject;
}
