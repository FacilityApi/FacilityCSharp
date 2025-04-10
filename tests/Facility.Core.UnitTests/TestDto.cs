using System.Diagnostics.CodeAnalysis;
using MessagePack;

namespace Facility.Core.UnitTests;

[MessagePackObject]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "Must be public for MessagePack.")]
public sealed class TestDto : ServiceDto<TestDto>
{
	[Key(0)]
	public int? Id { get; set; }

	[Key(1)]
	public string? Name { get; set; }

	[Key(2)]
	public IReadOnlyList<TestDto>? Children { get; set; }

	[Key(3)]
	[Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Include), ServiceNullableDefaultValue(typeof(ServiceNullable<bool?>))]
	[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
	public ServiceNullable<bool?> Ternary { get; set; }
}
