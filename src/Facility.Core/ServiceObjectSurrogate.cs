using ProtoBuf;

namespace Facility.Core;

[ProtoContract]
public class ServiceObjectSurrogate
{
	[ProtoMember(1)]
	public string JsonValue { get; set; } = default!;

	[ProtoConverter]
	public static ServiceObject? From(ServiceObjectSurrogate? value) =>
		value is null ? null : SystemTextJsonServiceSerializer.Instance.FromString<ServiceObject>(value.JsonValue);

	[ProtoConverter]
	public static ServiceObjectSurrogate? To(ServiceObject? value) =>
		value is null ? null : new ServiceObjectSurrogate { JsonValue = SystemTextJsonServiceSerializer.Instance.ToString(value)};
}
