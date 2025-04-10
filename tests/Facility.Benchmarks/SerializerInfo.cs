using Facility.Core;

namespace Facility.Benchmarks;

internal sealed record SerializerInfo(ServiceSerializer ServiceSerializer)
{
	public override string ToString() => ServiceSerializer.GetType().Name[0..^"ServiceSerializer".Length];
}
