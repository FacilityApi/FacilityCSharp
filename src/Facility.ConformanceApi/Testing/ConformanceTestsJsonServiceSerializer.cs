#if NET8_0_OR_GREATER
using Facility.Core;

namespace Facility.ConformanceApi.Testing;

internal sealed class ConformanceTestsJsonServiceSerializer : SystemTextJsonContextServiceSerializer
{
	public static readonly ConformanceTestsJsonServiceSerializer Instance = new();

	private ConformanceTestsJsonServiceSerializer()
		: base(new ConformanceTestsJsonSerializerContext(SystemTextJsonServiceSerializer.CreateJsonSerializerOptions()))
	{
	}
}
#endif
