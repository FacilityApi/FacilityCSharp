using System.Text.Json.Nodes;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

internal sealed class ServiceErrorDtoTests
{
	[Test]
	public void ToStringUsesJson()
	{
		s_error.ToString().Should().Be("""{"code":"Test","message":"Message.","details":{"Some":"details."},"innerError":{"code":"Inner"}}""");
	}

	[Test]
	public void BasicEquivalence()
	{
		var empty = new ServiceErrorDto();
		var full = s_error;

		empty.IsEquivalentTo(null).Should().BeFalse();
		empty.IsEquivalentTo(empty).Should().BeTrue();
		empty.IsEquivalentTo(new ServiceErrorDto()).Should().BeTrue();
		empty.IsEquivalentTo(full).Should().BeFalse();
		full.IsEquivalentTo(new ServiceErrorDto(s_error.Code)).Should().BeFalse();

		full.IsEquivalentTo(null).Should().BeFalse();
		full.IsEquivalentTo(empty).Should().BeFalse();
		full.IsEquivalentTo(new ServiceErrorDto(s_error.Code, s_error.Message) { DetailsObject = s_error.DetailsObject, InnerError = s_error.InnerError }).Should().BeTrue();
		full.IsEquivalentTo(full).Should().BeTrue();
		full.IsEquivalentTo(new ServiceErrorDto(s_error.Code)).Should().BeFalse();
	}

	[Test]
	public void GetDetailsDto()
	{
		var error = new ServiceErrorDto { DetailsObject = new JsonObject { { "booleanValue", true } } };
		error.GetDetails<ValueDto>()!.IsEquivalentTo(ValueDto.Create(true));
	}

	[Test]
	public void GetNullDetailsDto()
	{
		new ServiceErrorDto().GetDetails<ValueDto>().Should().BeNull();
	}

	[Test]
	public void SetDetailsDto()
	{
		var error = new ServiceErrorDto { DetailsObject = ValueDto.Create(true) };
		((bool) error.DetailsObject!.ToJObject()["booleanValue"]!).Should().BeTrue();
	}

	private static readonly ServiceErrorDto s_error = new("Test", "Message.") { DetailsObject = new JObject { ["Some"] = "details." }, InnerError = new ServiceErrorDto("Inner") };
}
