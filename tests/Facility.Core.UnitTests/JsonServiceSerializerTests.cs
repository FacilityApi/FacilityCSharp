using System.Text.Json.Nodes;
using Facility.Core.Assertions;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixtureSource(nameof(ServiceSerializers))]
public sealed class JsonServiceSerializerTests : ServiceSerializerTestBase
{
	public JsonServiceSerializerTests(ServiceSerializer serializer)
		: base(serializer)
	{
	}

	[Test]
	public void CamelCase()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"booleanValue\":true}";

		Serializer.ToString(dto).Should().Be(json);
		Serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void CamelCaseExceptDictionaryKeys()
	{
		var dto = ValueDto.Create(new Dictionary<string, bool> { ["Key"] = true });
		var json = "{\"booleanMapValue\":{\"Key\":true}}";

		Serializer.ToString(dto).Should().Be(json);
		Serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void DateParseHandlingNone()
	{
		var dto = ValueDto.Create("2016-10-21T15:31:00Z");
		var json = $"{{\"stringValue\":\"{dto.StringValue}\"}}";

		Serializer.ToString(dto).Should().Be(json);
		Serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void NullValueHandlingIgnore()
	{
		var dto = ValueDto.Create(default(bool?));
		var json = "{}";

		Serializer.ToString(dto).Should().Be(json);
		Serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void MissingMemberHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"booleanValue\":true,\"missing\":false}";
		Serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void MetadataPropertyHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"$ref\":\"xyzzy\",\"booleanValue\":true}";
		Serializer.FromString<ValueDto>(json).Should().BeDto(dto);
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

		Serializer.ToString(dto).Should().Be(json);
		Serializer.FromString<ValueDto>(json).Should().BeDto(dto);
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

		Serializer.ToString(dto).Should().Be(json);
		Serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void RoundTripFromJObject()
	{
		var so1 = ServiceObject.Create(new JObject { ["foo"] = "bar" });
		var so2 = Serializer.FromString<ServiceObject>(Serializer.ToString(so1));
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}

	[Test]
	public void RoundTripFromJsonObject()
	{
		var so1 = ServiceObject.Create(new JsonObject { ["foo"] = "bar" });
		var so2 = Serializer.FromString<ServiceObject>(Serializer.ToString(so1));
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}
}
