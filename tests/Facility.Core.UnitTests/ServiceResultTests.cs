using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Shouldly;

namespace Facility.Core.UnitTests
{
	public class ServiceResultTests
	{
		[Test]
		public void VoidSuccess()
		{
			var result = ServiceResult.Success();
			result.IsSuccess.ShouldBe(true);
			result.IsFailure.ShouldBe(false);
			result.Error.ShouldBe(null);
			result.Verify();
		}

		[Test]
		public void Int32Success()
		{
			var result = ServiceResult.Success(1);
			result.IsSuccess.ShouldBe(true);
			result.IsFailure.ShouldBe(false);
			result.Error.ShouldBe(null);
			result.Verify();
			result.Value.ShouldBe(1);
			result.GetValueOrDefault().ShouldBe(1);
		}

		[Test]
		public void NullSuccess()
		{
			var result = ServiceResult.Success((string) null);
			result.IsSuccess.ShouldBe(true);
			result.IsFailure.ShouldBe(false);
			result.Error.ShouldBe(null);
			result.Verify();
			result.Value.ShouldBe(null);
			result.GetValueOrDefault().ShouldBe(null);
		}

		[Test]
		public void FailureErrorMustNotBeNull()
		{
			Assert.Throws<ArgumentNullException>(() => ServiceResult.Failure(null));
		}

		[Test]
		public void EmptyFailure()
		{
			var result = ServiceResult.Failure(new ServiceErrorDto());
			result.IsSuccess.ShouldBe(false);
			result.IsFailure.ShouldBe(true);
			result.Error.ShouldBeEquivalent(new ServiceErrorDto());
			try
			{
				result.Verify();
				throw new InvalidOperationException();
			}
			catch (ServiceException exception)
			{
				exception.Error.ShouldBeEquivalent(new ServiceErrorDto());
			}
		}

		[Test]
		public void Int32Failure()
		{
			ServiceResult<int> result = ServiceResult.Failure(new ServiceErrorDto("Int32Failure"));
			result.IsSuccess.ShouldBe(false);
			result.IsFailure.ShouldBe(true);
			result.Error.ShouldBeEquivalent(new ServiceErrorDto("Int32Failure"));
			try
			{
				result.Verify();
				throw new InvalidOperationException();
			}
			catch (ServiceException exception)
			{
				exception.Error.ShouldBeEquivalent(new ServiceErrorDto("Int32Failure"));
			}
			try
			{
				result.Value.ShouldBe(0);
				throw new InvalidOperationException();
			}
			catch (ServiceException exception)
			{
				exception.Error.ShouldBeEquivalent(new ServiceErrorDto("Int32Failure"));
			}
		}

		[Test]
		public void AlwaysCastFailure()
		{
			ServiceResultFailure failure = ServiceResult.Failure(new ServiceErrorDto("Failure"));
			failure.Cast<int>().Error.ShouldBeEquivalent(new ServiceErrorDto("Failure"));
			ServiceResult noValue = ServiceResult.Failure(new ServiceErrorDto("NoValue"));
			noValue.Cast<int>().Error.ShouldBeEquivalent(new ServiceErrorDto("NoValue"));
			ServiceResult<string> stringValue = ServiceResult.Failure(new ServiceErrorDto("StringValue"));
			stringValue.Cast<int>().Error.ShouldBeEquivalent(new ServiceErrorDto("StringValue"));
		}

		[Test]
		public void ReferenceCasts()
		{
			ServiceResult<ArgumentException> result = ServiceResult.Success<ArgumentException>(new ArgumentNullException());
			result.Value.GetType().ShouldBe(typeof(ArgumentNullException));
			result.Cast<ArgumentNullException>().Value.GetType().ShouldBe(typeof(ArgumentNullException));
			result.Cast<ArgumentException>().Value.GetType().ShouldBe(typeof(ArgumentNullException));
			result.Cast<Exception>().Value.GetType().ShouldBe(typeof(ArgumentNullException));
			result.Cast<object>().Value.GetType().ShouldBe(typeof(ArgumentNullException));
			Assert.Throws<InvalidCastException>(() => result.Cast<InvalidOperationException>().Value.GetType().ShouldBe(typeof(ArgumentNullException)));
		}

		[Test]
		public void ValueCasts()
		{
			ServiceResult<long> result = ServiceResult.Success(1L);
			result.Value.GetType().ShouldBe(typeof(long));
			result.Cast<long>().Value.GetType().ShouldBe(typeof(long));
			Assert.Throws<InvalidCastException>(() => result.Cast<int>().Value.ShouldBe(1));
		}

		[Test]
		public void NullCasts()
		{
			ServiceResult<ArgumentException> result = ServiceResult.Success<ArgumentException>(null);
			result.Cast<InvalidOperationException>().Value.ShouldBe(null);
			result.Cast<long?>().Value.ShouldBe(null);
		}

		[Test]
		public void NoValueCasts()
		{
			ServiceResult result = ServiceResult.Success();
			Assert.Throws<InvalidCastException>(() => result.Cast<object>());
		}

		[Test]
		public void FailureAsFailure()
		{
			var error = new ServiceErrorDto("Error");
			ServiceResultFailure failure = ServiceResult.Failure(error);
			failure.AsFailure().Error.ShouldBe(error);
			ServiceResult failedResult = ServiceResult.Failure(error);
			failedResult.AsFailure().Error.ShouldBe(error);
			ServiceResult<int> failedValue = ServiceResult.Failure(error);
			failedValue.AsFailure().Error.ShouldBe(error);
		}

		[Test]
		public void SuccessAsFailure()
		{
			ServiceResult successResult = ServiceResult.Success();
			successResult.AsFailure().ShouldBe(null);
			ServiceResult<int> successValue = ServiceResult.Success(1);
			successValue.AsFailure().ShouldBe(null);
		}

		[Test]
		public void MapFailure()
		{
			var error = new ServiceErrorDto("Error");
			ServiceResult<int> failedValue = ServiceResult.Failure(error);
			failedValue.Map(x => x.ToString()).Error.ShouldBe(error);
		}

		[Test]
		public void MapSuccess()
		{
			ServiceResult<int> successValue = ServiceResult.Success(1);
			successValue.Map(x => x.ToString()).Value.ShouldBe("1");
		}

		[Test]
		public void NoValueSuccessJson()
		{
			var before = ServiceResult.Success();
			string json = ServiceJsonUtility.ToJson(before);
			json.ShouldBe("{}");
			var after = ServiceJsonUtility.FromJson<ServiceResult>(json);
			after.ShouldBeEquivalent(before);
		}

		[Test]
		public void NoValueEmptyFailureJson()
		{
			var before = ServiceResult.Failure(new ServiceErrorDto());
			string json = ServiceJsonUtility.ToJson(before);
			json.ShouldBe("{\"error\":{}}");
			var after = ServiceJsonUtility.FromJson<ServiceResult>(json);
			after.ShouldBeEquivalent(before);
		}

		[Test]
		public void NoValueFailureJson()
		{
			var before = ServiceResult.Failure(new ServiceErrorDto("Xyzzy", "Xyzzy unexpected."));
			string json = ServiceJsonUtility.ToJson(before);
			json.ShouldBe("{\"error\":{\"code\":\"Xyzzy\",\"message\":\"Xyzzy unexpected.\"}}");
			var after = ServiceJsonUtility.FromJson<ServiceResult>(json);
			after.ShouldBeEquivalent(before);
		}

		[Test]
		public void IntegerSuccessJson()
		{
			var before = ServiceResult.Success(1337);
			string json = ServiceJsonUtility.ToJson(before);
			json.ShouldBe("{\"value\":1337}");
			var after = ServiceJsonUtility.FromJson<ServiceResult<int>>(json);
			after.ShouldBeEquivalent(before);
		}

		[Test]
		public void IntegerEmptyFailureJson()
		{
			var before = ServiceResult.Failure(new ServiceErrorDto());
			string json = ServiceJsonUtility.ToJson(before);
			json.ShouldBe("{\"error\":{}}");
			var after = ServiceJsonUtility.FromJson<ServiceResult<int?>>(json);
			after.ShouldBeEquivalent(before);
		}

		[Test]
		public void IntegerFailureJson()
		{
			var before = ServiceResult.Failure(new ServiceErrorDto("Xyzzy", "Xyzzy unexpected."));
			string json = ServiceJsonUtility.ToJson(before);
			json.ShouldBe("{\"error\":{\"code\":\"Xyzzy\",\"message\":\"Xyzzy unexpected.\"}}");
			var after = ServiceJsonUtility.FromJson<ServiceResult<int>>(json);
			after.ShouldBeEquivalent(before);
		}

		[Test]
		public void NullIntegerSuccessJson()
		{
			var before = ServiceResult.Success(default(int?));
			string json = ServiceJsonUtility.ToJson(before);
			json.ShouldBe("{\"value\":null}");
			var after = ServiceJsonUtility.FromJson<ServiceResult<int?>>(json);
			after.ShouldBeEquivalent(before);
		}

		[Test]
		public void MissingValueIntegerSuccessJson()
		{
			var before = ServiceResult.Success(default(int?));
			const string json = "{}";
			var after = ServiceJsonUtility.FromJson<ServiceResult<int?>>(json);
			after.ShouldBeEquivalent(before);
		}

		[Test]
		public void ValueAndErrorThrows()
		{
			const string json = "{\"value\":1337,\"error\":{}}";
			Assert.Throws<JsonSerializationException>(() => ServiceJsonUtility.FromJson<ServiceResult<int?>>(json));
		}

		[Test]
		public void ExtraFieldSuccessJson()
		{
			var before = ServiceResult.Success(1337);
			string json = "{\"values\":1337,\"value\":1337,\"valuex\":1337}";
			var after = ServiceJsonUtility.FromJson<ServiceResult<int>>(json);
			after.ShouldBeEquivalent(before);
		}

		[Test]
		public void ExtraFieldFailureJson()
		{
			var before = ServiceResult.Failure(new ServiceErrorDto());
			string json = "{\"values\":1337,\"error\":{},\"valuex\":1337}";
			var after = ServiceJsonUtility.FromJson<ServiceResult<int>>(json);
			after.ShouldBeEquivalent(before);
		}
	}
}
