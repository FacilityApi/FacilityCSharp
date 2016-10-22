using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace Facility.Core.UnitTests
{
	public sealed class ServiceDataUtilityTests
	{
		[Fact]
		public void CloneArray()
		{
			var invalidRequest = new ServiceErrorDto { Code = ServiceErrors.InvalidRequest };
			var invalidResponse = new ServiceErrorDto { Code = ServiceErrors.InvalidResponse };
			var dto = CreateValueDto((IReadOnlyList<ServiceErrorDto>) new List<ServiceErrorDto>
			{
				invalidRequest,
				invalidResponse,
			});
			string json = "{\"value\":[{\"code\":\"InvalidRequest\"},{\"code\":\"InvalidResponse\"}]}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson(json, dto.GetType()).ShouldBe(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["value"].Type.ShouldBe(JTokenType.Array);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void CloneDictionary()
		{
			var invalidRequest = new ServiceErrorDto { Code = ServiceErrors.InvalidRequest };
			var invalidResponse = new ServiceErrorDto { Code = ServiceErrors.InvalidResponse };
			var dto = CreateValueDto((IReadOnlyDictionary<string, ServiceErrorDto>) new Dictionary<string, ServiceErrorDto>
			{
				["request"] = invalidRequest,
				["response"] = invalidResponse,
			});

			string json = "{\"value\":{\"request\":{\"code\":\"InvalidRequest\"},\"response\":{\"code\":\"InvalidResponse\"}}}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson(json, dto.GetType()).ShouldBe(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["value"].Type.ShouldBe(JTokenType.Object);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		private ValueDto<T> CreateValueDto<T>(T value)
		{
			return new ValueDto<T> { Value = value };
		}

		private sealed class ValueDto<T> : ServiceDto<ValueDto<T>>
		{
			public T Value { get; set; }

			public override bool IsEquivalentTo(ValueDto<T> other)
			{
				return ToString() == other?.ToString();
			}
		}
	}
}
