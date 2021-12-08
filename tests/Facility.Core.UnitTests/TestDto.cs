namespace Facility.Core.UnitTests;

[MessagePack.MessagePackObject]
public class TestDto : ServiceDto<TestDto>
{
	[MessagePack.Key(0)]
	public int? Id { get; set; }

	[MessagePack.Key(1)]
	public string? Name { get; set; }

	[MessagePack.Key(2)]
	public IReadOnlyList<TestDto>? Children { get; set; }

	public override bool IsEquivalentTo(TestDto? other)
	{
		return other != null &&
			other.Id == Id &&
			other.Name == Name &&
			ServiceDataUtility.AreEquivalentArrays(other.Children, Children, ServiceDataUtility.AreEquivalentDtos);
	}
}
