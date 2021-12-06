using ProtoBuf;

namespace Facility.Core;

[ProtoContract]
public class ServiceResultSurrogate
{
	[ProtoMember(1)]
	public ServiceErrorDto? Error { get; set; }

	[ProtoConverter]
	public static ServiceResult? From(ServiceResultSurrogate? value) =>
		value is null ? null :
		value.Error is { } error ? (ServiceResult) ServiceResult.Failure(error) :
		ServiceResult.Success();

	[ProtoConverter]
	public static ServiceResultSurrogate? To(ServiceResult? value) =>
		value is null ? null : new ServiceResultSurrogate { Error = value.Error };

	[ProtoConverter]
	public static ServiceResultSurrogate? To(ServiceResultFailure? value) =>
		value is null ? null : new ServiceResultSurrogate { Error = value.Error };
}

[ProtoContract]
public class ServiceResultSurrogate<T>
{
	[ProtoMember(1)]
	public ServiceErrorDto? Error { get; set; }

	[ProtoMember(2)]
	public T? Value { get; set; }

#pragma warning disable CA1000
	[ProtoConverter]
	public static ServiceResult<T>? From(ServiceResultSurrogate<T>? value) =>
		value is null ? null :
		value.Error is { } error ? (ServiceResult<T>) ServiceResult.Failure(error) :
		ServiceResult.Success(value.Value!);

	[ProtoConverter]
	public static ServiceResultSurrogate<T>? To(ServiceResult<T>? value) =>
		value is null ? null : new ServiceResultSurrogate<T> { Error = value.Error, Value = value.GetValueOrDefault() };
#pragma warning restore CA1000
}
