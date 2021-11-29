using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Facility.Core;

public sealed class SystemTextJsonServiceSerializer : ServiceSerializer
{
	public static readonly SystemTextJsonServiceSerializer Instance = new();

	public override string ToString(object? value) => JsonSerializer.Serialize(value, s_jsonSerializerOptions);

	public override void ToStream(object? value, Stream outputStream) => JsonSerializer.Serialize(outputStream, value, s_jsonSerializerOptions);

	public override object? FromString(string stringValue, Type type) => JsonSerializer.Deserialize(stringValue, type, s_jsonSerializerOptions);

	public override object? FromStream(Stream stream, Type type) => JsonSerializer.Deserialize(stream, type, s_jsonSerializerOptions);

	public override ServiceObject? ToServiceObject(object? value) => value is null ? null :
		ServiceObject.Create((JsonObject) JsonSerializer.SerializeToNode(value, s_jsonSerializerOptions)!);

	public override object? FromServiceObject(ServiceObject? serviceObject, Type type) => serviceObject?.AsJsonObject().Deserialize(type, s_jsonSerializerOptions);

	private static readonly JsonSerializerOptions s_jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		Converters =
		{
			new ServiceResultSystemTextJsonConverter(),
		},
	};
}
