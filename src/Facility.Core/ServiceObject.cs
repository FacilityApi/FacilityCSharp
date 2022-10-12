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
	/// Returns the JSON object as a <c>Newtonsoft.Json.Linq.JObject</c>.
	/// </summary>
	[Obsolete("Use overload with ServiceObjectAccess.")]
	public JObject AsJObject() => AsJObject(ServiceObjectAccess.ReadOnly);

	/// <summary>
	/// Returns the JSON object as a <c>Newtonsoft.Json.Linq.JObject</c>.
	/// </summary>
	public JObject AsJObject(ServiceObjectAccess access)
	{
		if (access is not (ServiceObjectAccess.Clone or ServiceObjectAccess.ReadWrite or ServiceObjectAccess.ReadOnly))
			throw new ArgumentOutOfRangeException(nameof(access), access, null);

		if (access is ServiceObjectAccess.Clone || m_object is not JObject jObject)
			jObject = NewtonsoftJsonServiceSerializer.Instance.FromJson<JObject>(ToString())!;

		if (access is ServiceObjectAccess.ReadWrite)
			m_object = jObject;

		return jObject;
	}

	/// <summary>
	/// Returns the JSON object as a new <c>System.Text.Json.Nodes.JsonObject</c>.
	/// </summary>
	[Obsolete("Use overload with ServiceObjectAccess.")]
	public JsonObject AsJsonObject() => AsJsonObject(ServiceObjectAccess.ReadOnly);

	/// <summary>
	/// Returns the JSON object as a <c>System.Text.Json.Nodes.JsonObject</c>.
	/// </summary>
	public JsonObject AsJsonObject(ServiceObjectAccess access)
	{
		if (access is not (ServiceObjectAccess.Clone or ServiceObjectAccess.ReadWrite or ServiceObjectAccess.ReadOnly))
			throw new ArgumentOutOfRangeException(nameof(access), access, null);

		if (access is ServiceObjectAccess.Clone || m_object is not JsonObject jsonObject)
			jsonObject = SystemTextJsonServiceSerializer.Instance.FromJson<JsonObject>(ToString())!;

		if (access is ServiceObjectAccess.ReadWrite)
			m_object = jsonObject;

		return jsonObject;
	}

	/// <summary>
	/// Returns true if the JSON objects are equivalent.
	/// </summary>
	public bool IsEquivalentTo(ServiceObject? other) => other is not null &&
		m_object switch
		{
			JObject jObject => JToken.DeepEquals(jObject, other.AsJObject(ServiceObjectAccess.ReadOnly)),
			JsonObject jsonObject => SystemTextJsonUtility.DeepEquals(jsonObject, other.AsJsonObject(ServiceObjectAccess.ReadOnly)),
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
