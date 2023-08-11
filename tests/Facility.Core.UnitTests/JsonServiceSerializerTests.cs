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
		const string json = """{"booleanValue":true}""";

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void CamelCaseExceptDictionaryKeys()
	{
		var dto = ValueDto.Create(new Dictionary<string, bool> { ["Key"] = true });
		const string json = """{"booleanMapValue":{"Key":true}}""";

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void DateParseHandlingNone()
	{
		var dto = ValueDto.Create("2016-10-21T15:31:00Z");
		var json = $$"""{"stringValue":"{{dto.StringValue}}"}""";

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void NullValueHandlingIgnore()
	{
		var dto = ValueDto.Create(default(bool?));
		const string json = "{}";

		JsonSerializer.ToJson(dto).Should().Be(json);
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void MissingMemberHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		const string json = """{"booleanValue":true,"missing":false}""";
		JsonSerializer.FromJson<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void MetadataPropertyHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		const string json = """{"$ref":"xyzzy","booleanValue":true}""";
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
		const string json = """{"errorArrayValue":[{"code":"InvalidRequest"},{"code":"InvalidResponse"}]}""";

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

		const string json = """{"errorMapValue":{"request":{"code":"InvalidRequest"},"response":{"code":"InvalidResponse"}}}""";

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
		Assert.AreEqual("hi", (string) JsonSerializer.FromJson<JValue>(JsonSerializer.ToJson((JValue) "hi"))!);
		Assert.IsTrue((bool) JsonSerializer.FromJson<JToken>(JsonSerializer.ToJson((JToken) true))!);
	}

	[Test]
	public void AllowReadingIntegerFromString()
	{
		JsonSerializer.FromJson<ValueDto>("""{"integerValue":"42"}""").Should().BeDto(ValueDto.Create(42));
	}

	[Test]
	public void AllowReadingDoubleFromString()
	{
		JsonSerializer.FromJson<ValueDto>("""{"doubleValue":"42"}""").Should().BeDto(ValueDto.Create(42.0));
		JsonSerializer.FromJson<ValueDto>("""{"doubleValue":"6.825"}""").Should().BeDto(ValueDto.Create(6.825));
		JsonSerializer.FromJson<ValueDto>("""{"doubleValue":"Infinity"}""").Should().BeDto(ValueDto.Create(double.PositiveInfinity));
		JsonSerializer.FromJson<ValueDto>("""{"doubleValue":"-Infinity"}""").Should().BeDto(ValueDto.Create(double.NegativeInfinity));
		JsonSerializer.FromJson<ValueDto>("""{"doubleValue":"NaN"}""").Should().BeDto(ValueDto.Create(double.NaN));
	}

	[Test]
	public void AllowReadingBooleanFromString()
	{
		JsonSerializer.FromJson<ValueDto>("""{"booleanValue":"true"}""").Should().BeDto(ValueDto.Create(true));
		JsonSerializer.FromJson<ValueDto>("""{"booleanValue":"false"}""").Should().BeDto(ValueDto.Create(false));
		JsonSerializer.FromJson<ValueDto>("""{"booleanValue":null}""").Should().BeDto(new ValueDto());
	}

	[Test]
	public void AllowReadingStringFromNonString()
	{
		JsonSerializer.FromJson<ValueDto>("""{"stringValue":true}""").Should().BeDto(ValueDto.Create("true"));
		JsonSerializer.FromJson<ValueDto>("""{"stringValue":false}""").Should().BeDto(ValueDto.Create("false"));
		JsonSerializer.FromJson<ValueDto>("""{"stringValue":42}""").Should().BeDto(ValueDto.Create("42"));
		JsonSerializer.FromJson<ValueDto>("""{"stringValue":6.825}""").Should().BeDto(ValueDto.Create("6.825"));
		JsonSerializer.FromJson<ValueDto>("""{"stringValue":null}""").Should().BeDto(new ValueDto());
	}

	[Test]
	public void DateTimeToJson()
	{
		var input = ValueDto.Create(new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc));
		JsonSerializer.ToJson(input).Should().Be("""{"dateTimeValue":"2001-02-03T04:05:06Z"}""");
	}

	[Test]
	public void DateTimeFromJson()
	{
		var output = JsonSerializer.FromJson<ValueDto>("""{"dateTimeValue":"2001-02-03T04:05:06Z"}""");
		output.Should().BeDto(ValueDto.Create(new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc)));
		output!.DateTimeValue!.Value.Kind.Should().Be(DateTimeKind.Utc);
	}

	[Test]
	public void DateTimeToJsonWithMilliseconds()
	{
		var input = ValueDto.Create(new DateTime(2001, 2, 3, 4, 5, 6, 7, DateTimeKind.Utc));
		JsonSerializer.ToJson(input).Should().Be("""{"dateTimeValue":"2001-02-03T04:05:06Z"}""");
	}

	[Test]
	public void DateTimeFromJsonWithMilliseconds()
	{
		Assert.Throws<ServiceSerializationException>(
			() => JsonSerializer.FromJson<ValueDto>("""{"dateTimeValue":"2001-02-03T04:05:06.007Z"}"""));
	}

	[Test]
	public void DateTimeEquivalenceWithMilliseconds()
	{
		ValueDto.Create(new DateTime(2001, 2, 3, 4, 5, 6, 1, DateTimeKind.Utc))
			.IsEquivalentTo(ValueDto.Create(new DateTime(2001, 2, 3, 4, 5, 6, 999, DateTimeKind.Utc)))
			.Should().BeTrue();
	}

	[TestCase(DateTimeKind.Local)]
	[TestCase(DateTimeKind.Unspecified)]
	public void DateTimeNotUtc(DateTimeKind kind)
	{
		Assert.Throws<ServiceSerializationException>(
			() => JsonSerializer.ToJson(ValueDto.Create(new DateTime(2001, 2, 3, 4, 5, 6, kind))));
	}
}
