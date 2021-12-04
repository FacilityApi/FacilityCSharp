using Facility.Core;

namespace Facility.ConformanceApi.UnitTests;

public abstract class ServiceSerializerTestBase
{
	public static IReadOnlyList<ServiceSerializer> ServiceSerializers => new ServiceSerializer[]
	{
		NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
		ProtobufServiceSerializer.Instance,
	};

	protected ServiceSerializerTestBase(ServiceSerializer serializer)
	{
		Serializer = serializer;
	}

	protected ServiceSerializer Serializer { get; }
}
