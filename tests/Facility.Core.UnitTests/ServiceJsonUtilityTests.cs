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
			var dto = ValueDto.Create(true);
			string json = "{\"booleanValue\":true}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson<ValueDto>(json).ShouldBeEquivalent(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["booleanValue"].Type.ShouldBe(JTokenType.Boolean);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void CamelCaseExceptDictionaryKeys()
		{
			var dto = ValueDto.Create(new Dictionary<string, bool> { ["Key"] = true });
			string json = "{\"booleanMapValue\":{\"Key\":true}}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson<ValueDto>(json).ShouldBeEquivalent(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["booleanMapValue"].Type.ShouldBe(JTokenType.Object);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void DateParseHandlingNone()
		{
			var dto = ValueDto.Create("2016-10-21T15:31:00Z");
			string json = $"{{\"stringValue\":\"{dto.StringValue}\"}}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson<ValueDto>(json).ShouldBeEquivalent(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["stringValue"].Type.ShouldBe(JTokenType.String);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void NullValueHandlingIgnore()
		{
			var dto = ValueDto.Create(default(bool?));
			string json = "{}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson<ValueDto>(json).ShouldBeEquivalent(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["stringValue"].ShouldBe(null);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void MissingMemberHandlingIgnore()
		{
			var dto = ValueDto.Create(true);
			string json = "{\"booleanValue\":true,\"missing\":false}";
			ServiceJsonUtility.FromJson<ValueDto>(json).ShouldBeEquivalent(dto);
		}

		[Fact]
		public void MetadataPropertyHandlingIgnore()
		{
			var dto = ValueDto.Create(true);
			string json = "{\"$ref\":\"xyzzy\",\"booleanValue\":true}";
			ServiceJsonUtility.FromJson<ValueDto>(json).ShouldBeEquivalent(dto);
		}

		[Fact]
		public void ArraySerialization()
		{
			var invalidRequest = new ServiceErrorDto { Code = ServiceErrors.InvalidRequest };
			var invalidResponse = new ServiceErrorDto { Code = ServiceErrors.InvalidResponse };
			var dto = ValueDto.Create(new List<ServiceErrorDto>
			{
				invalidRequest,
				invalidResponse,
			});
			string json = "{\"errorArrayValue\":[{\"code\":\"InvalidRequest\"},{\"code\":\"InvalidResponse\"}]}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson<ValueDto>(json).ShouldBeEquivalent(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["errorArrayValue"].Type.ShouldBe(JTokenType.Array);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}

		[Fact]
		public void DictionarySerialization()
		{
			var invalidRequest = new ServiceErrorDto { Code = ServiceErrors.InvalidRequest };
			var invalidResponse = new ServiceErrorDto { Code = ServiceErrors.InvalidResponse };
			var dto = ValueDto.Create(new Dictionary<string, ServiceErrorDto>
			{
				["request"] = invalidRequest,
				["response"] = invalidResponse,
			});

			string json = "{\"errorMapValue\":{\"request\":{\"code\":\"InvalidRequest\"},\"response\":{\"code\":\"InvalidResponse\"}}}";

			ServiceJsonUtility.ToJson(dto).ShouldBe(json);
			ServiceJsonUtility.FromJson<ValueDto>(json).ShouldBeEquivalent(dto);

			var token = ServiceJsonUtility.FromJson<JToken>(json);
			token["errorMapValue"].Type.ShouldBe(JTokenType.Object);
			ServiceJsonUtility.ToJson(token).ShouldBe(json);
		}
	}
}
