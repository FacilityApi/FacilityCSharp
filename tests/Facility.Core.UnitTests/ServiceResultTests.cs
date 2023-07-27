using System.Globalization;
using Facility.Core.Assertions;
using FluentAssertions;
using NUnit.Framework;
using static FluentAssertions.FluentActions;

namespace Facility.Core.UnitTests;

public class ServiceResultTests
{
	[Test]
	public void VoidSuccess()
	{
		var result = ServiceResult.Success();
		result.IsSuccess.Should().BeTrue();
		result.IsFailure.Should().BeFalse();
		result.Error!.Should().BeNull();
		result.Verify();
	}

	[Test]
	public void Int32Success()
	{
		var result = ServiceResult.Success(1);
		result.IsSuccess.Should().BeTrue();
		result.IsFailure.Should().BeFalse();
		result.Error!.Should().BeNull();
		result.Verify();
		result.Value.Should().Be(1);
		result.GetValueOrDefault().Should().Be(1);
	}

	[Test]
	public void NullSuccess()
	{
		var result = ServiceResult.Success((string?) null);
		result.IsSuccess.Should().BeTrue();
		result.IsFailure.Should().BeFalse();
		result.Error!.Should().BeNull();
		result.Verify();
		result.Value.Should().BeNull();
		result.GetValueOrDefault().Should().BeNull();
	}

	[Test]
	public void FailureErrorMustNotBeNull()
	{
		Assert.Throws<ArgumentNullException>(() => ServiceResult.Failure(null!));
	}

	[Test]
	public void EmptyFailure()
	{
		var result = ServiceResult.Failure(new ServiceErrorDto());
		result.IsSuccess.Should().BeFalse();
		result.IsFailure.Should().BeTrue();
		result.Error!.Should().BeDto(new ServiceErrorDto());
		try
		{
			result.Verify();
			throw new InvalidOperationException();
		}
		catch (ServiceException exception)
		{
			exception.Error.Should().BeDto(new ServiceErrorDto());
		}
	}

	[Test]
	public void Int32Failure()
	{
		ServiceResult<int> result = ServiceResult.Failure(new ServiceErrorDto("Int32Failure"));
		result.IsSuccess.Should().BeFalse();
		result.IsFailure.Should().BeTrue();
		result.Error!.Should().BeDto(new ServiceErrorDto("Int32Failure"));
		try
		{
			result.Verify();
			throw new InvalidOperationException();
		}
		catch (ServiceException exception)
		{
			exception.Error.Should().BeDto(new ServiceErrorDto("Int32Failure"));
		}
		try
		{
			result.Value.Should().Be(0);
			throw new InvalidOperationException();
		}
		catch (ServiceException exception)
		{
			exception.Error.Should().BeDto(new ServiceErrorDto("Int32Failure"));
		}
	}

	[Test]
	public void AlwaysCastFailure()
	{
		var failure = ServiceResult.Failure(new ServiceErrorDto("Failure"));
		failure.Cast<int>().Error!.Should().BeDto(new ServiceErrorDto("Failure"));
		ServiceResult noValue = ServiceResult.Failure(new ServiceErrorDto("NoValue"));
		noValue.Cast<int>().Error!.Should().BeDto(new ServiceErrorDto("NoValue"));
		ServiceResult<string> stringValue = ServiceResult.Failure(new ServiceErrorDto("StringValue"));
		stringValue.Cast<int>().Error!.Should().BeDto(new ServiceErrorDto("StringValue"));
	}

	[Test]
	public void ReferenceCasts()
	{
		var result = ServiceResult.Success<ArgumentException>(new ArgumentNullException());
		result.Value.GetType().Should().Be(typeof(ArgumentNullException));
		result.Cast<ArgumentNullException>().Value.GetType().Should().Be(typeof(ArgumentNullException));
		result.Cast<ArgumentException>().Value.GetType().Should().Be(typeof(ArgumentNullException));
		result.Cast<Exception>().Value.GetType().Should().Be(typeof(ArgumentNullException));
		result.Cast<object>().Value.GetType().Should().Be(typeof(ArgumentNullException));
		Assert.Throws<InvalidCastException>(() => result.Cast<InvalidOperationException>().Value.GetType().Should().Be(typeof(ArgumentNullException)));
	}

	[Test]
	public void ValueCasts()
	{
		var result = ServiceResult.Success(1L);
		result.Value.GetType().Should().Be(typeof(long));
		result.Cast<long>().Value.GetType().Should().Be(typeof(long));
		Assert.Throws<InvalidCastException>(() => result.Cast<int>().Value.Should().Be(1));
	}

	[Test]
	public void NullCasts()
	{
		var result = ServiceResult.Success<ArgumentException>(null!);
		result.Cast<InvalidOperationException>().Value.Should().BeNull();
		result.Cast<long?>().Value.Should().BeNull();
	}

	[Test]
	public void NoValueCasts()
	{
		var result = ServiceResult.Success();
		Assert.Throws<InvalidCastException>(() => result.Cast<object>());
	}

	[Test]
	public void FailureAsFailure()
	{
		var error = new ServiceErrorDto("Error");
		var failure = ServiceResult.Failure(error);
		failure.AsFailure()!.Error!.Should().BeDto(error);
		ServiceResult failedResult = ServiceResult.Failure(error);
		failedResult.AsFailure()!.Error!.Should().BeDto(error);
		ServiceResult<int> failedValue = ServiceResult.Failure(error);
		failedValue.AsFailure()!.Error!.Should().BeDto(error);
	}

	[Test]
	public void SuccessAsFailure()
	{
		var successResult = ServiceResult.Success();
		successResult.AsFailure().Should().BeNull();
		var successValue = ServiceResult.Success(1);
		successValue.AsFailure().Should().BeNull();
	}

	[Test]
	public void FailureToFailure()
	{
		var error = new ServiceErrorDto("Error");
		var failure = ServiceResult.Failure(error);
		failure.ToFailure().Error!.Should().BeDto(error);
		ServiceResult failedResult = ServiceResult.Failure(error);
		failedResult.ToFailure().Error!.Should().BeDto(error);
		ServiceResult<int> failedValue = ServiceResult.Failure(error);
		failedValue.ToFailure().Error!.Should().BeDto(error);
	}

	[Test]
	public void SuccessToFailure()
	{
		var successResult = ServiceResult.Success();
		Invoking(successResult.ToFailure).Should().Throw<InvalidOperationException>();
		var successValue = ServiceResult.Success(1);
		Invoking(successValue.ToFailure).Should().Throw<InvalidOperationException>();
	}

	[Test]
	public void MapFailure()
	{
		var error = new ServiceErrorDto("Error");
		ServiceResult<int> failedValue = ServiceResult.Failure(error);
		failedValue.Map(x => x.ToString(CultureInfo.InvariantCulture)).Error.Should().BeDto(error);
	}

	[Test]
	public void MapSuccess()
	{
		var successValue = ServiceResult.Success(1);
		successValue.Map(x => x.ToString(CultureInfo.InvariantCulture)).Value.Should().Be("1");
	}
}
