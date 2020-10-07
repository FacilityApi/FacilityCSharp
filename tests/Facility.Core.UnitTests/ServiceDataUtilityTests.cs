using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests
{
	public sealed class ServiceDataUtilityTests
	{
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

			var clone = ServiceDataUtility.Clone(dto);
			clone.Should().NotBeSameAs(dto);
			clone.ErrorMapValue.Should().NotBeSameAs(dto.ErrorMapValue);
			clone.IsEquivalentTo(dto).Should().Be(true);
		}
	}
}
