using System.Text.Json.Nodes;
using Facility.Core.Assertions;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixture(typeof(NewtonsoftJsonServiceSerializer))]
[TestFixture(typeof(SystemTextJsonServiceSerializer))]
public sealed class JsonServiceSerializerTests
{
	public JsonServiceSerializerTests(Type serializerType)
	{
		m_serializer = (ServiceSerializer) Activator.CreateInstance(serializerType)!;
	}

	[Test]
	public void CamelCase()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"booleanValue\":true}";

		m_serializer.ToString(dto).Should().Be(json);
		m_serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void CamelCaseExceptDictionaryKeys()
	{
		var dto = ValueDto.Create(new Dictionary<string, bool> { ["Key"] = true });
		var json = "{\"booleanMapValue\":{\"Key\":true}}";

		m_serializer.ToString(dto).Should().Be(json);
		m_serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void DateParseHandlingNone()
	{
		var dto = ValueDto.Create("2016-10-21T15:31:00Z");
		var json = $"{{\"stringValue\":\"{dto.StringValue}\"}}";

		m_serializer.ToString(dto).Should().Be(json);
		m_serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void NullValueHandlingIgnore()
	{
		var dto = ValueDto.Create(default(bool?));
		var json = "{}";

		m_serializer.ToString(dto).Should().Be(json);
		m_serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void MissingMemberHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"booleanValue\":true,\"missing\":false}";
		m_serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void MetadataPropertyHandlingIgnore()
	{
		var dto = ValueDto.Create(true);
		var json = "{\"$ref\":\"xyzzy\",\"booleanValue\":true}";
		m_serializer.FromString<ValueDto>(json).Should().BeDto(dto);
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

		m_serializer.ToString(dto).Should().Be(json);
		m_serializer.FromString<ValueDto>(json).Should().BeDto(dto);
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

		m_serializer.ToString(dto).Should().Be(json);
		m_serializer.FromString<ValueDto>(json).Should().BeDto(dto);
	}

	[Test]
	public void RoundTripFromJObject()
	{
		var so1 = ServiceObject.Create(new JObject { ["foo"] = "bar" });
		var so2 = m_serializer.FromString<ServiceObject>(m_serializer.ToString(so1));
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}

	[Test]
	public void RoundTripFromJsonObject()
	{
		var so1 = ServiceObject.Create(new JsonObject { ["foo"] = "bar" });
		var so2 = m_serializer.FromString<ServiceObject>(m_serializer.ToString(so1));
		Assert.IsTrue(so1.IsEquivalentTo(so2));
	}

	private readonly ServiceSerializer m_serializer;
}
