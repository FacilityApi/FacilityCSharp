using Facility.Core;

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
}
