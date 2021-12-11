using Facility.Core;
using Facility.Core.MessagePack;

namespace Facility.ConformanceApi.UnitTests;

public abstract class ServiceSerializerTestsBase
{
	public static IReadOnlyList<ServiceSerializer> ServiceSerializers => new ServiceSerializer[]
	{
		NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
		MessagePackServiceSerializer.Instance,
	};

	protected ServiceSerializerTestsBase(ServiceSerializer serializer)
	{
		Serializer = serializer;
		JsonSerializer = serializer as JsonServiceSerializer ?? NewtonsoftJsonServiceSerializer.Instance;
	}

	protected ServiceSerializer Serializer { get; }

	protected JsonServiceSerializer JsonSerializer { get; }
}
