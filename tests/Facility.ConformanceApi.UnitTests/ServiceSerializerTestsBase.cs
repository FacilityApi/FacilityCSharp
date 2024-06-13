using System.Collections.Generic;
using Facility.Core;
using Facility.Core.MessagePack;

namespace Facility.ConformanceApi.UnitTests;

public abstract class ServiceSerializerTestsBase
{
	public static IReadOnlyList<ServiceSerializer> ServiceSerializers =>
	[
		NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
		MessagePackServiceSerializer.Instance,
#if NET8_0_OR_GREATER
		ConformanceApiJsonServiceSerializer.Instance,
#endif
	];

	protected ServiceSerializerTestsBase(ServiceSerializer serializer)
	{
		Serializer = serializer;
		JsonSerializer = serializer as JsonServiceSerializer ?? NewtonsoftJsonServiceSerializer.Instance;
	}

	protected ServiceSerializer Serializer { get; }

	protected JsonServiceSerializer JsonSerializer { get; }
}
