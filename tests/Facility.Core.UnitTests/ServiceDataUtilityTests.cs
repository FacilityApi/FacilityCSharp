using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixtureSource(nameof(ServiceSerializers))]
internal sealed class ServiceDataUtilityTests : ServiceSerializerTestsBase
{
	public ServiceDataUtilityTests(ServiceSerializer serializer)
		: base(serializer)
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

		var clone = Serializer.Clone(dto);
		clone.Should().NotBeSameAs(dto);
		clone.ErrorMapValue.Should().NotBeSameAs(dto.ErrorMapValue);
		clone.IsEquivalentTo(dto).Should().Be(true);
	}

	[Test]
	public void ServiceResultClone()
	{
		var result = ServiceResult.Success(ValueDto.Create(true));

		var clone = Serializer.Clone(result);
		clone.Should().NotBeSameAs(result);
		ServiceDataUtility.AreEquivalentResults(clone, result).Should().Be(true);
	}

	[Test]
	public void ServiceResultNoValueClone()
	{
		var result = ServiceResult.Success();

		var clone = Serializer.Clone(result);
		clone.Should().NotBeSameAs(result);
		ServiceDataUtility.AreEquivalentResults(clone, result).Should().Be(true);
	}

	[Test]
	public void ServiceResultFailureClone()
	{
		var result = ServiceResult.Failure(ServiceErrors.CreateConflict());

		var clone = Serializer.Clone(result);
		clone.Should().NotBeSameAs(result);
		ServiceDataUtility.AreEquivalentResults(clone, result).Should().Be(true);
	}
}
