namespace Facility.Core.UnitTests;

public abstract class JsonServiceSerializerTestsBase
{
	public static IReadOnlyList<JsonServiceSerializer> JsonServiceSerializers =>
	[
		NewtonsoftJsonServiceSerializer.Instance,
		SystemTextJsonServiceSerializer.Instance,
	];

	protected JsonServiceSerializerTestsBase(JsonServiceSerializer jsonSerializer)
	{
		JsonSerializer = jsonSerializer;
	}

	protected JsonServiceSerializer JsonSerializer { get; }
}
