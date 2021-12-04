using Facility.Core.Assertions;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixtureSource(nameof(JsonServiceSerializers))]
public class JsonServiceResultTests : ServiceSerializerTestBase
{
	public JsonServiceResultTests(ServiceSerializer serializer)
		: base(serializer)
	{
	}

	[Test]
	public void NoValueSuccessJson()
	{
		var before = ServiceResult.Success();
		var json = Serializer.ToString(before);
		json.Should().Be("{}");
		var after = Serializer.FromString<ServiceResult>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void NoValueEmptyFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto());
		var json = Serializer.ToString(before);
		json.Should().Be("{\"error\":{}}");
		var after = Serializer.FromString<ServiceResult>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void NoValueFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto("Xyzzy", "Xyzzy unexpected."));
		var json = Serializer.ToString(before);
		json.Should().Be("{\"error\":{\"code\":\"Xyzzy\",\"message\":\"Xyzzy unexpected.\"}}");
		var after = Serializer.FromString<ServiceResult>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void IntegerSuccessJson()
	{
		var before = ServiceResult.Success(1337);
		var json = Serializer.ToString(before);
		json.Should().Be("{\"value\":1337}");
		var after = Serializer.FromString<ServiceResult<int>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void IntegerEmptyFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto());
		var json = Serializer.ToString(before);
		json.Should().Be("{\"error\":{}}");
		var after = Serializer.FromString<ServiceResult<int?>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void IntegerFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto("Xyzzy", "Xyzzy unexpected."));
		var json = Serializer.ToString(before);
		json.Should().Be("{\"error\":{\"code\":\"Xyzzy\",\"message\":\"Xyzzy unexpected.\"}}");
		var after = Serializer.FromString<ServiceResult<int>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void NullIntegerSuccessJson()
	{
		var before = ServiceResult.Success(default(int?));
		var json = Serializer.ToString(before);
		json.Should().Be("{\"value\":null}");
		var after = Serializer.FromString<ServiceResult<int?>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void MissingValueIntegerSuccessJson()
	{
		var before = ServiceResult.Success(default(int?));
		const string json = "{}";
		var after = Serializer.FromString<ServiceResult<int?>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void ValueAndErrorThrows()
	{
		const string json = "{\"value\":1337,\"error\":{}}";
		Assert.Throws<ServiceSerializationException>(() => Serializer.FromString<ServiceResult<int?>>(json));
	}

	[Test]
	public void ExtraFieldSuccessJson()
	{
		var before = ServiceResult.Success(1337);
		var json = "{\"values\":1337,\"value\":1337,\"valuex\":1337}";
		var after = Serializer.FromString<ServiceResult<int>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void ExtraFieldFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto());
		var json = "{\"values\":1337,\"error\":{},\"valuex\":1337}";
		var after = Serializer.FromString<ServiceResult<int>>(json);
		after.Should().BeResult(before);
	}
}
