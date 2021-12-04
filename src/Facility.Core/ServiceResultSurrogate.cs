using ProtoBuf;

namespace Facility.Core;

[ProtoContract]
public class ServiceResultFailureSurrogate
{
	[ProtoMember(1)]
	public ServiceErrorDto? Error { get; set; }

	[ProtoConverter]
	public static ServiceResultFailure? From(ServiceResultFailureSurrogate? value) =>
		value is null ? null : ServiceResult.Failure(value.Error!);

	[ProtoConverter]
	public static ServiceResultFailureSurrogate? To(ServiceResultFailure? value) =>
		value is null ? null : new ServiceResultFailureSurrogate { Error = value.Error };
}
