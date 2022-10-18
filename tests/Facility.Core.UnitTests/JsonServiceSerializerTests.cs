using System.Text.Json.Nodes;
using Facility.Core.Assertions;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixtureSource(nameof(JsonServiceSerializers))]
public sealed class JsonServiceSerializerTests : JsonServiceSerializerTestsBase
{
	public JsonServiceSerializerTests(JsonServiceSerializer jsonSerializer)
		: base(jsonSerializer)
	{
	}

	[Test]
	public void CamelCase()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"booleanValue\":true}";

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void CamelCaseExceptDictionaryKeys()
	{
		var dto = ValueDto.Create(new Dictionary<string, bool> { ["Key"] = true });
		var json = "{\"booleanMapValue\":{\"Key\":true}}";

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void DateParseHandlingNone()
	{
		var dto = ValueDto.Create("2016-10-21T15:31:00Z");
		var json = $"{{\"stringValue\":\"{dto.StringValue}\"}}";

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void NullValueHandlingIgnore()
	{
		var dto = ValueDto.Create(default(bool?));
		var json = "{}";

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void MissingMemberHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"booleanValue\":true,\"missing\":false}";
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void MetadataPropertyHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"$ref\":\"xyzzy\",\"booleanValue\":true}";
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
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

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
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

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void RoundTripFromJObject()
	{
		var so1 = ServiceObject.Create(new JObject { ["foo"] = "bar" });
		var so2 = JsonSerializer.FromJson<ServiceObject>(JsonSerializer.ToJson(so1));
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}

	[Test]
	public void RoundTripFromJObjectWithDate()
	{
		var so1 = ServiceObject.Create(new JObject { ["foo"] = "2022-10-18T09:15:08.3426473-07:00" });
		var so2 = JsonSerializer.FromJson<ServiceObject>(JsonSerializer.ToJson(so1));
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}

	[Test]
	public void RoundTripFromJsonObject()
	{
		var so1 = ServiceObject.Create(new JsonObject { ["foo"] = "bar" });
		var so2 = JsonSerializer.FromJson<ServiceObject>(JsonSerializer.ToJson(so1));
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}

	[Test]
	public void RoundTripFromJsonObjectWithDate()
	{
		var so1 = ServiceObject.Create(new JsonObject { ["foo"] = "2022-10-18T09:15:08.3426473-07:00" });
		var so2 = JsonSerializer.FromJson<ServiceObject>(JsonSerializer.ToJson(so1));
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}

	[Test]
	public void RoundTripJObject()
	{
		var legacy1 = new LegacyObjectDto { Extra = new JObject { ["foo"] = "bar" } };
		var legacy2 = JsonSerializer.FromJson<LegacyObjectDto>(JsonSerializer.ToJson(legacy1));
		Assert.IsTrue(legacy1.IsEquivalentTo(legacy2));
	}

	[Test]
	public void RoundTripJObjectWithDate()
	{
		var legacy1 = new LegacyObjectDto { Extra = new JObject { ["foo"] = "2022-10-18T09:15:08.3426473-07:00" } };
		var legacy2 = JsonSerializer.FromJson<LegacyObjectDto>(JsonSerializer.ToJson(legacy1));
		Assert.IsTrue(legacy1.IsEquivalentTo(legacy2));
	}

	[Test]
	public void RoundTripNullJObject()
	{
		var legacy1 = new LegacyObjectDto();
		var legacy2 = JsonSerializer.FromJson<LegacyObjectDto>(JsonSerializer.ToJson(legacy1));
		Assert.IsTrue(legacy1.IsEquivalentTo(legacy2));
	}

	[Test]
	public void RoundTripJToken()
	{
		Assert.AreEqual(0, JsonSerializer.FromJson<JObject>(JsonSerializer.ToJson(new JObject()))!.Count);
		Assert.AreEqual(0, JsonSerializer.FromJson<JArray>(JsonSerializer.ToJson(new JArray()))!.Count);
		Assert.AreEqual("hi", (string) JsonSerializer.FromJson<JValue>(JsonSerializer.ToJson((JValue) "hi")));
		Assert.IsTrue((bool) JsonSerializer.FromJson<JToken>(JsonSerializer.ToJson((JToken) true)));
	}
}
