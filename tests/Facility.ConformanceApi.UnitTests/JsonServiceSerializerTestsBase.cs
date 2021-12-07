using Facility.Core;
using Microsoft.IO;

namespace Facility.ConformanceApi.UnitTests;

public abstract class JsonServiceSerializerTestsBase
{
	public static IReadOnlyList<JsonServiceSerializer> JsonServiceSerializers => new JsonServiceSerializer[]
	{
		NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
	};

	protected JsonServiceSerializerTestsBase(JsonServiceSerializer jsonSerializer)
	{
		JsonSerializer = jsonSerializer;
	}

	protected JsonServiceSerializer JsonSerializer { get; }

	protected static RecyclableMemoryStreamManager MemoryStreamManager { get; } = new();
}
