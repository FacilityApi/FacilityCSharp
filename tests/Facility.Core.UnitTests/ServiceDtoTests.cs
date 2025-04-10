using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixtureSource(nameof(ServiceSerializers))]
internal sealed class ServiceDtoTests : ServiceSerializerTestsBase
{
	public ServiceDtoTests(ServiceSerializer serializer)
		: base(serializer)
	{
	}

	[Test]
	public void ToStringUsesJson()
	{
		var dto = new TestDto { Id = 3, Name = "Three", Children = [new TestDto { Name = "child" }], Ternary = null };
		var json = """{"id":3,"name":"Three","children":[{"name":"child"}],"ternary":null}""";
		dto.ToString().Should().Be(json);

		if (Serializer is JsonServiceSerializer jsonSerializer)
			jsonSerializer.ToJson(dto).Should().Be(json);
	}

	[Test]
	public void BasicEquivalence()
	{
		var empty = new TestDto();
		var full = new TestDto { Id = 3, Name = "Three", Children = [new TestDto { Name = "child" }], Ternary = null };

		empty.IsEquivalentTo(null).Should().BeFalse();
		empty.IsEquivalentTo(empty).Should().BeTrue();
		empty.IsEquivalentTo(new TestDto()).Should().BeTrue();
		empty.IsEquivalentTo(full).Should().BeFalse();
		full.IsEquivalentTo(new TestDto { Id = 3 }).Should().BeFalse();

		full.IsEquivalentTo(null).Should().BeFalse();
		full.IsEquivalentTo(empty).Should().BeFalse();
		full.IsEquivalentTo(new TestDto { Id = 3, Name = "Three", Children = [new TestDto { Name = "child" }], Ternary = null }).Should().BeTrue();
		full.IsEquivalentTo(full).Should().BeTrue();
		full.IsEquivalentTo(new TestDto { Id = 3 }).Should().BeFalse();
	}

	[Test]
	public void BasicCloning()
	{
		var first = new TestDto { Id = 3, Name = "Three", Children = [new TestDto { Name = "child" }], Ternary = null };
		var second = Serializer.Clone(first);
		first.IsEquivalentTo(second).Should().Be(true);
		second.Id += 1;
		first.IsEquivalentTo(second).Should().Be(false);
	}

	[Test]
	public async Task DateTimeRoundTrip()
	{
		var before = new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);
		var after = await DateTimeRoundTripAsync(before);
		after.Should().Be(before);
	}

	[Test]
	public async Task DateTimeRoundTripWithMilliseconds()
	{
		var before = new DateTime(2001, 2, 3, 4, 5, 6, 7, DateTimeKind.Utc);
		var after = await DateTimeRoundTripAsync(before);
		after.Should().Be(new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc));
	}

	[TestCase(DateTimeKind.Local)]
	[TestCase(DateTimeKind.Unspecified)]
	public async Task DateTimeNotUtc(DateTimeKind kind)
	{
		var input = ValueDto.Create(new DateTime(2001, 2, 3, 4, 5, 6, kind));
		using var stream = new MemoryStream();
		Assert.ThrowsAsync<ServiceSerializationException>(
			async () => await Serializer.ToStreamAsync(input, stream, CancellationToken.None));
	}

	private async Task<DateTime?> DateTimeRoundTripAsync(DateTime value)
	{
		var input = ValueDto.Create(value);
		using var stream = new MemoryStream();
		await Serializer.ToStreamAsync(input, stream, CancellationToken.None);
		stream.Position = 0;
		var output = (ValueDto) (await Serializer.FromStreamAsync(stream, typeof(ValueDto), CancellationToken.None))!;
		return output.DateTimeValue;
	}
}
