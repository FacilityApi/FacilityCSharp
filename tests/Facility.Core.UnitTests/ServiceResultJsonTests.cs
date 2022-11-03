using Facility.Core.Assertions;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests;

[TestFixtureSource(nameof(JsonServiceSerializers))]
public class ServiceResultJsonTests : JsonServiceSerializerTestsBase
{
	public ServiceResultJsonTests(JsonServiceSerializer jsonSerializer)
		: base(jsonSerializer)
	{
	}

	[Test]
	public void NoValueSuccessJson()
	{
		var before = ServiceResult.Success();
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{}");
		var after = JsonSerializer.FromJson<ServiceResult>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void NoValueEmptyFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto());
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{\"error\":{}}");
		var after = JsonSerializer.FromJson<ServiceResult>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void NoValueFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto("Xyzzy", "Xyzzy unexpected."));
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{\"error\":{\"code\":\"Xyzzy\",\"message\":\"Xyzzy unexpected.\"}}");
		var after = JsonSerializer.FromJson<ServiceResult>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void IntegerSuccessJson()
	{
		var before = ServiceResult.Success(1337);
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{\"value\":1337}");
		var after = JsonSerializer.FromJson<ServiceResult<int>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void IntegerEmptyFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto());
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{\"error\":{}}");
		var after = JsonSerializer.FromJson<ServiceResult<int?>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void IntegerFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto("Xyzzy", "Xyzzy unexpected."));
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{\"error\":{\"code\":\"Xyzzy\",\"message\":\"Xyzzy unexpected.\"}}");
		var after = JsonSerializer.FromJson<ServiceResult<int>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void NullIntegerSuccessJson()
	{
		var before = ServiceResult.Success(default(int?));
		var json = JsonSerializer.ToJson(before);
		json.Should().Be("{\"value\":null}");
		var after = JsonSerializer.FromJson<ServiceResult<int?>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void MissingValueIntegerSuccessJson()
	{
		var before = ServiceResult.Success(default(int?));
		const string json = "{}";
		var after = JsonSerializer.FromJson<ServiceResult<int?>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void ValueAndErrorThrows()
	{
		const string json = "{\"value\":1337,\"error\":{}}";
		Assert.Throws<ServiceSerializationException>(() => JsonSerializer.FromJson<ServiceResult<int?>>(json));
	}

	[Test]
	public void ExtraFieldSuccessJson()
	{
		var before = ServiceResult.Success(1337);
		var json = "{\"values\":1337,\"value\":1337,\"valuex\":1337}";
		var after = JsonSerializer.FromJson<ServiceResult<int>>(json);
		after.Should().BeResult(before);
	}

	[Test]
	public void ExtraFieldFailureJson()
	{
		var before = ServiceResult.Failure(new ServiceErrorDto());
		var json = "{\"values\":1337,\"error\":{},\"valuex\":1337}";
		var after = JsonSerializer.FromJson<ServiceResult<int>>(json);
		after.Should().BeResult(before);
	}
}
