namespace Facility.Core.UnitTests;

public abstract class ServiceSerializerTestBase
{
	public static IReadOnlyList<ServiceSerializer> JsonServiceSerializers => new ServiceSerializer[]
	{
		NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
	};

	public static IReadOnlyList<ServiceSerializer> ServiceSerializers => new ServiceSerializer[]
	{
		NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
		ProtobufServiceSerializer.Instance,
		MessagePackServiceSerializer.Instance,
	};

	protected ServiceSerializerTestBase(ServiceSerializer serializer)
	{
		Serializer = serializer;
	}

	protected ServiceSerializer Serializer { get; }
}
