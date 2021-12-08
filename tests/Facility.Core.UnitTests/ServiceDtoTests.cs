using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixtureSource(nameof(ServiceSerializers))]
public class ServiceDtoTests : ServiceSerializerTestsBase
{
	public ServiceDtoTests(ServiceSerializer serializer)
		: base(serializer)
	{
	}

	[Test]
	public void ToStringUsesJson()
	{
		var dto = new TestDto { Id = 3, Name = "Three", Children = new[] { new TestDto { Name = "child" } } };
		var json = @"{""id"":3,""name"":""Three"",""children"":[{""name"":""child""}]}";
		dto.ToString().Should().Be(json);

		if (Serializer is JsonServiceSerializer jsonSerializer)
			jsonSerializer.ToJson(dto).Should().Be(json);
	}

	[Test]
	public void BasicEquivalence()
	{
		var empty = new TestDto();
		var full = new TestDto { Id = 3, Name = "Three", Children = new[] { new TestDto { Name = "child" } } };

		empty.IsEquivalentTo(null).Should().BeFalse();
		empty.IsEquivalentTo(empty).Should().BeTrue();
		empty.IsEquivalentTo(new TestDto()).Should().BeTrue();
		empty.IsEquivalentTo(full).Should().BeFalse();
		full.IsEquivalentTo(new TestDto { Id = 3 }).Should().BeFalse();

		full.IsEquivalentTo(null).Should().BeFalse();
		full.IsEquivalentTo(empty).Should().BeFalse();
		full.IsEquivalentTo(new TestDto { Id = 3, Name = "Three", Children = new[] { new TestDto { Name = "child" } } }).Should().BeTrue();
		full.IsEquivalentTo(full).Should().BeTrue();
		full.IsEquivalentTo(new TestDto { Id = 3 }).Should().BeFalse();
	}

	[Test]
	public void BasicCloning()
	{
		var first = new TestDto { Id = 3, Name = "Three", Children = new[] { new TestDto { Name = "child" } } };
		var second = Serializer.Clone(first);
		first.IsEquivalentTo(second).Should().Be(true);
		second.Id += 1;
		first.IsEquivalentTo(second).Should().Be(false);
	}
}
