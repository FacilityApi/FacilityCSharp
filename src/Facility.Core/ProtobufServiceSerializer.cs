using ProtoBuf;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

namespace Facility.Core;

public sealed class ProtobufServiceSerializer : ServiceSerializer
{
	public static readonly ProtobufServiceSerializer Instance = new();

	public override string DefaultMediaType => "application/protobuf";

	public override string ToString(object? value) => Convert.ToBase64String(ToBytes(value));

	public override void ToStream(object? value, Stream outputStream) => Serializer.Serialize(outputStream, value);

	public override object? FromString(string stringValue, Type type)
	{
		byte[] bytesValue;
		try
		{
			bytesValue = Convert.FromBase64String(stringValue);
		}
		catch (Exception exception)
		{
			throw new ServiceSerializationException(exception);
		}
		return FromBytes(bytesValue, type);
	}

	public override object? FromStream(Stream stream, Type type) => Serializer.Deserialize(type, stream);

	public override ServiceObject? ToServiceObject(object? value) => SystemTextJsonServiceSerializer.Instance.ToServiceObject(value);

	public override object? FromServiceObject(ServiceObject? serviceObject, Type type) => SystemTextJsonServiceSerializer.Instance.FromServiceObject(serviceObject, type);

	static ProtobufServiceSerializer()
	{
		RuntimeTypeModel.Default.Add(typeof(ServiceObject), false)
			.SetSurrogate(typeof(ServiceObjectSurrogate));

		RuntimeTypeModel.Default.Add(typeof(ServiceResult), false)
			.SetSurrogate(typeof(ServiceResultSurrogate));

		RuntimeTypeModel.Default.Add(typeof(ServiceResultFailure), false)
			.SetSurrogate(typeof(ServiceResultSurrogate));
	}
}
