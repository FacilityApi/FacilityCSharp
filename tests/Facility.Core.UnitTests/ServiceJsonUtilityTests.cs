using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace Facility.Core.UnitTests
{
	public sealed class ServiceJsonUtilityTests
	{
		[Fact]
		public void CamelCase()
		{
			var dto = CreateValueDto(true);
			string json = "{\"value\":true}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson(json, dto.GetType()).ShouldBe(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["value"].Type.ShouldBe(JTokenType.Boolean);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void CamelCaseExceptDictionaryKeys()
		{
			var dto = CreateValueDto(new Dictionary<string, bool> { ["Key"] = true });
			string json = "{\"value\":{\"Key\":true}}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson(json, dto.GetType()).ShouldBe(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["value"].Type.ShouldBe(JTokenType.Object);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void DateParseHandlingNone()
		{
			var dto = CreateValueDto("2016-10-21T15:31:00Z");
			string json = $"{{\"value\":\"{dto.Value}\"}}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson(json, dto.GetType()).ShouldBe(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["value"].Type.ShouldBe(JTokenType.String);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void NullValueHandlingIgnore()
		{
			var dto = CreateValueDto(default(bool?));
			string json = "{}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson(json, dto.GetType()).ShouldBe(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["value"].ShouldBe(null);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void MissingMemberHandlingIgnore()
		{
			var dto = CreateValueDto(true);
			string json = "{\"value\":true,\"missing\":false}";
			ServiceJsonUtility.FromJson(json, dto.GetType()).ShouldBe(dto);
		}

		[Fact]
		public void MetadataPropertyHandlingIgnore()
		{
			var dto = CreateValueDto(true);
			string json = "{\"$ref\":\"xyzzy\",\"value\":true}";
			ServiceJsonUtility.FromJson(json, dto.GetType()).ShouldBe(dto);
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
