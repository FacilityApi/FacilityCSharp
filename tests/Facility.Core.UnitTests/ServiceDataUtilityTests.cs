using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

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
			clone.ShouldNotBeSameAs(dto);
			clone.ErrorMapValue.ShouldNotBeSameAs(dto.ErrorMapValue);
			clone.IsEquivalentTo(dto).ShouldBe(true);
		}
	}
}
