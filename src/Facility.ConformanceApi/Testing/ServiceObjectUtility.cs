using Facility.Core;

namespace Facility.ConformanceApi.Testing;

internal static class ServiceObjectUtility
{
	public static bool DeepEquals(ServiceObject? so1, ServiceObject? so2) => so1?.IsEquivalentTo(so2) ?? so2 is null;
}
