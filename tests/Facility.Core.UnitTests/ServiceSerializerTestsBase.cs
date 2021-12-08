namespace Facility.Core.UnitTests;

public abstract class ServiceSerializerTestsBase
{
	public static IReadOnlyList<ServiceSerializer> ServiceSerializers => new ServiceSerializer[]
	{
		NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
	};

	protected ServiceSerializerTestsBase(ServiceSerializer serializer)
	{
		Serializer = serializer;
	}

	protected ServiceSerializer Serializer { get; }
}
