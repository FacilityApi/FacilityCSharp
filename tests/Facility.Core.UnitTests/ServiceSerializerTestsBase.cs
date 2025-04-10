using Facility.Core.MessagePack;

namespace Facility.Core.UnitTests;

internal abstract class ServiceSerializerTestsBase
{
	public static IReadOnlyList<ServiceSerializer> ServiceSerializers =>
	[
		NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
		MessagePackServiceSerializer.Instance,
	];

	protected ServiceSerializerTestsBase(ServiceSerializer serializer)
	{
		Serializer = serializer;
	}

	protected ServiceSerializer Serializer { get; }
}
