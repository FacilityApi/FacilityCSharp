using Facility.Core.Assertions;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixtureSource(nameof(JsonServiceSerializers))]
internal sealed class ServiceNullableJsonTests : JsonServiceSerializerTestsBase
{
	public ServiceNullableJsonTests(JsonServiceSerializer jsonSerializer)
		: base(jsonSerializer)
	{
	}

	[Test]
	public void DefaultJson()
	{
		var before = default(ServiceNullable<string?>);
		Assert.Throws<InvalidOperationException>(() => JsonSerializer.ToJson(before));
	}

	[Test]
	public void NullJson()
	{
		var before = new ServiceNullable<string?>(null);
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("null");
		var after = JsonSerializer.FromJson<ServiceNullable<string?>>(json);
		after.Should().Be(before);
	}

	[Test]
	public void NotNullJson()
	{
		var before = new ServiceNullable<string?>("42");
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("\"42\"");
		var after = JsonSerializer.FromJson<ServiceNullable<string?>>(json);
		after.Should().Be(before);
	}

	[Test]
	public void DefaultTernaryJson()
	{
		var before = new TestDto { Ternary = default };
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{}");
		var after = JsonSerializer.FromJson<TestDto>(json)!;
		after.Should().BeDto(before);
		after.Ternary.IsUnspecified.Should().BeTrue();
		after.Ternary.Value.Should().BeNull();
	}

	[Test]
	public void NullTernaryJson()
	{
		var before = new TestDto { Ternary = null };
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{\"ternary\":null}");
		var after = JsonSerializer.FromJson<TestDto>(json)!;
		after.Should().BeDto(before);
		after.Ternary.IsUnspecified.Should().BeFalse();
		after.Ternary.Value.Should().BeNull();
	}

	[Test]
	public void FalseTernaryJson()
	{
		var before = new TestDto { Ternary = false };
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{\"ternary\":false}");
		var after = JsonSerializer.FromJson<TestDto>(json)!;
		after.Should().BeDto(before);
		after.Ternary.IsUnspecified.Should().BeFalse();
		after.Ternary.Value.Should().BeFalse();
	}

	[Test]
	public void TrueTernaryJson()
	{
		var before = new TestDto { Ternary = true };
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{\"ternary\":true}");
		var after = JsonSerializer.FromJson<TestDto>(json)!;
		after.Should().BeDto(before);
		after.Ternary.IsUnspecified.Should().BeFalse();
		after.Ternary.Value.Should().BeTrue();
	}
}
