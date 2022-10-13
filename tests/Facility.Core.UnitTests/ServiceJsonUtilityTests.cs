using System.Text.Json.Nodes;
using Facility.Core.Assertions;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

#pragma warning disable 618 // obsolete

namespace Facility.Core.UnitTests;

public sealed class ServiceJsonUtilityTests
{
	[Test]
	public void CamelCase()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"booleanValue\":true}";

		ServiceJsonUtility.ToJson(dto).Should().Be(json);
		ServiceJsonUtility.FromJson<ValueDto>(json).Should().BeDto(dto);

		var token = ServiceJsonUtility.FromJson<JToken>(json);
		token["booleanValue"].Type.Should().Be(JTokenType.Boolean);
		ServiceJsonUtility.ToJson(token).Should().Be(json);
	}

	[Test]
	public void CamelCaseExceptDictionaryKeys()
	{
		var dto = ValueDto.Create(new Dictionary<string, bool> { ["Key"] = true });
		var json = "{\"booleanMapValue\":{\"Key\":true}}";

		ServiceJsonUtility.ToJson(dto).Should().Be(json);
		ServiceJsonUtility.FromJson<ValueDto>(json).Should().BeDto(dto);

		var token = ServiceJsonUtility.FromJson<JToken>(json);
		token["booleanMapValue"].Type.Should().Be(JTokenType.Object);
		ServiceJsonUtility.ToJson(token).Should().Be(json);
	}

	[Test]
	public void DateParseHandlingNone()
	{
		var dto = ValueDto.Create("2016-10-21T15:31:00Z");
		var json = $"{{\"stringValue\":\"{dto.StringValue}\"}}";

		ServiceJsonUtility.ToJson(dto).Should().Be(json);
		ServiceJsonUtility.FromJson<ValueDto>(json).Should().BeDto(dto);

		var token = ServiceJsonUtility.FromJson<JToken>(json);
		token["stringValue"].Type.Should().Be(JTokenType.String);
		ServiceJsonUtility.ToJson(token).Should().Be(json);
	}

	[Test]
	public void NullValueHandlingIgnore()
	{
		var dto = ValueDto.Create(default(bool?));
		var json = "{}";

		ServiceJsonUtility.ToJson(dto).Should().Be(json);
		ServiceJsonUtility.FromJson<ValueDto>(json).Should().BeDto(dto);

		var token = ServiceJsonUtility.FromJson<JToken>(json);
		token["stringValue"].Should().BeNull();
		ServiceJsonUtility.ToJson(token).Should().Be(json);
	}

	[Test]
	public void MissingMemberHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"booleanValue\":true,\"missing\":false}";
		ServiceJsonUtility.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void MetadataPropertyHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"$ref\":\"xyzzy\",\"booleanValue\":true}";
		ServiceJsonUtility.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void ArraySerialization()
	{
		var invalidRequest = new ServiceErrorDto { Code = ServiceErrors.InvalidRequest };
		var invalidResponse = new ServiceErrorDto { Code = ServiceErrors.InvalidResponse };
		var dto = ValueDto.Create(new List<ServiceErrorDto>
		{
			invalidRequest,
			invalidResponse,
		});
		var json = "{\"errorArrayValue\":[{\"code\":\"InvalidRequest\"},{\"code\":\"InvalidResponse\"}]}";

		ServiceJsonUtility.ToJson(dto).Should().Be(json);
		ServiceJsonUtility.FromJson<ValueDto>(json).Should().BeDto(dto);

		var token = ServiceJsonUtility.FromJson<JToken>(json);
		token["errorArrayValue"].Type.Should().Be(JTokenType.Array);
		ServiceJsonUtility.ToJson(token).Should().Be(json);
	}

	[Test]
	public void DictionarySerialization()
	{
		var invalidRequest = new ServiceErrorDto { Code = ServiceErrors.InvalidRequest };
		var invalidResponse = new ServiceErrorDto { Code = ServiceErrors.InvalidResponse };
		var dto = ValueDto.Create(new Dictionary<string, ServiceErrorDto>
		{
			["request"] = invalidRequest,
			["response"] = invalidResponse,
		});

		var json = "{\"errorMapValue\":{\"request\":{\"code\":\"InvalidRequest\"},\"response\":{\"code\":\"InvalidResponse\"}}}";

		ServiceJsonUtility.ToJson(dto).Should().Be(json);
		ServiceJsonUtility.FromJson<ValueDto>(json).Should().BeDto(dto);

		var token = ServiceJsonUtility.FromJson<JToken>(json);
		token["errorMapValue"].Type.Should().Be(JTokenType.Object);
		ServiceJsonUtility.ToJson(token).Should().Be(json);
	}

	[Test]
	public void ServiceObjects([Values] bool legacy)
	{
		var so = legacy ? ServiceObject.Create(new JObject { ["foo"] = "bar" }) : ServiceObject.Create(new JsonObject { ["foo"] = "bar" });
		var json = @"{""foo"":""bar""}";

		ServiceJsonUtility.ToJson(so).Should().Be(json);
		ServiceJsonUtility.FromJson<ServiceObject>(json).IsEquivalentTo(so).Should().BeTrue();
	}
}
