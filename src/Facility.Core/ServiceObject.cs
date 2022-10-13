using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace Facility.Core;

/// <summary>
/// Encapsulates a JSON object.
/// </summary>
[Newtonsoft.Json.JsonConverter(typeof(ServiceObjectNewtonsoftJsonConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(ServiceObjectSystemTextJsonConverter))]
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
	/// Returns a <c>Newtonsoft.Json.Linq.JObject</c> that is temporarily associated with this <c>ServiceObject</c>.
	/// </summary>
	/// <remarks>If the returned object is mutated, the <c>ServiceObject</c> is mutated. However, once the
	/// <c>ServiceObject</c> is accessed in any other way, including comparison or serialization, the object may
	/// or may not remain associated with the <c>ServiceObject</c> and thus should no longer be used.</remarks>
	public JObject AsJObject()
	{
		if (m_object is not JObject jObject)
			m_object = jObject = ToJObject();
		return jObject;
	}

	/// <summary>
	/// Returns a <c>System.Text.Json.Nodes.JsonObject</c> that is temporarily associated with this <c>ServiceObject</c>.
	/// </summary>
	/// <remarks>If the returned object is mutated, the <c>ServiceObject</c> is mutated. However, once the
	/// <c>ServiceObject</c> is accessed in any other way, including comparison or serialization, the object may
	/// or may not remain associated with the <c>ServiceObject</c> and thus should no longer be used.</remarks>
	public JsonObject AsJsonObject()
	{
		if (m_object is not JsonObject jsonObject)
			m_object = jsonObject = ToJsonObject();
		return jsonObject;
	}

	/// <summary>
	/// Returns a new <c>System.Text.Json.Nodes.JsonObject</c> equivalent to this <c>ServiceObject</c>.
	/// </summary>
	public JObject ToJObject() => NewtonsoftJsonServiceSerializer.Instance.FromJson<JObject>(ToString())!;

	/// <summary>
	/// Returns a new <c>System.Text.Json.Nodes.JsonObject</c> equivalent to this <c>ServiceObject</c>.
	/// </summary>
	public JsonObject ToJsonObject() => SystemTextJsonServiceSerializer.Instance.FromJson<JsonObject>(ToString())!;

	/// <summary>
	/// Returns true if the JSON objects are equivalent.
	/// </summary>
	public bool IsEquivalentTo(ServiceObject? other) => other is not null &&
		m_object switch
		{
			JObject jObject => JToken.DeepEquals(jObject, (other.m_object as JObject) ?? other.ToJObject()),
			JsonObject jsonObject => SystemTextJsonUtility.DeepEquals(jsonObject, (other.m_object as JsonObject) ?? other.AsJsonObject()),
			_ => throw new InvalidOperationException(),
		};

	/// <summary>
	/// Returns a JSON string for the JSON object.
	/// </summary>
	public override string ToString() =>
		m_object switch
		{
			JObject jObject => NewtonsoftJsonServiceSerializer.Instance.ToJson(jObject),
			JsonObject jsonObject => SystemTextJsonServiceSerializer.Instance.ToJson(jsonObject),
			_ => "",
		};

	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Create is the named alternative.")]
	public static implicit operator ServiceObject?(JObject? jObject) => Create(jObject);

	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Create is the named alternative.")]
	public static implicit operator ServiceObject?(JsonObject? jsonObject) => Create(jsonObject);

	private ServiceObject(JObject jObject) => m_object = jObject;

	private ServiceObject(JsonObject jsonObject) => m_object = jsonObject;

	private object m_object;
}
