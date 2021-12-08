namespace Facility.Core.UnitTests;

public class TestDto : ServiceDto<TestDto>
{
	public int? Id { get; set; }

	public string? Name { get; set; }

	public IReadOnlyList<TestDto>? Children { get; set; }

	public override bool IsEquivalentTo(TestDto? other)
	{
		return other != null &&
			other.Id == Id &&
			other.Name == Name &&
			ServiceDataUtility.AreEquivalentArrays(other.Children, Children, ServiceDataUtility.AreEquivalentDtos);
	}
}
