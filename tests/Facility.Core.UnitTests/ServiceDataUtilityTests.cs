using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixtureSource(nameof(JsonServiceSerializers))]
public sealed class ServiceDataUtilityTests : JsonServiceSerializerTestsBase
{
	public ServiceDataUtilityTests(JsonServiceSerializer jsonSerializer)
		: base(jsonSerializer)
	{
	}

	[Test]
	public void DictionaryClone()
	{
		var invalidRequest = new ServiceErrorDto { Code = ServiceErrors.InvalidRequest };
		var invalidResponse = new ServiceErrorDto { Code = ServiceErrors.InvalidResponse };
		var dto = ValueDto.Create(new Dictionary<string, ServiceErrorDto>
		{
			["request"] = invalidRequest,
			["response"] = invalidResponse,
		});

		var clone = ServiceDataUtility.Clone(dto, JsonSerializer);
		clone.Should().NotBeSameAs(dto);
		clone.ErrorMapValue.Should().NotBeSameAs(dto.ErrorMapValue);
		clone.IsEquivalentTo(dto).Should().Be(true);
	}
}
