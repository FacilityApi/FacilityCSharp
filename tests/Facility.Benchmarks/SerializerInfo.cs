using Facility.Core;

namespace Facility.Benchmarks;

public record SerializerInfo(ServiceSerializer ServiceSerializer)
{
	public override string ToString() => ServiceSerializer.GetType().Name[0..^"ServiceSerializer".Length];
}
