using MessagePack;

namespace Facility.Core.UnitTests;

[MessagePackObject]
public class TestDto : ServiceDto<TestDto>
{
	[Key(0)]
	public int? Id { get; set; }

	[Key(1)]
	public string? Name { get; set; }

	[Key(2)]
	public IReadOnlyList<TestDto>? Children { get; set; }

	public override bool IsEquivalentTo(TestDto? other)
	{
		return other != null &&
			other.Id == Id &&
			other.Name == Name &&
			ServiceDataUtility.AreEquivalentArrays(other.Children, Children, ServiceDataUtility.AreEquivalentDtos);
	}
}
