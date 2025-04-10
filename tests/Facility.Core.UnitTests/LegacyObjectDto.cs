using Newtonsoft.Json.Linq;

namespace Facility.Core.UnitTests;

internal sealed class LegacyObjectDto : ServiceDto<LegacyObjectDto>
{
	public JObject? Extra { get; set; }

	public override bool IsEquivalentTo(LegacyObjectDto? other) =>
		other != null && ServiceDataUtility.AreEquivalentObjects(other.Extra, Extra);
}
