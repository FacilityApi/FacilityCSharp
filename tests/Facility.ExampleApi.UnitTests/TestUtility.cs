using System;
using System.Net.Http;
using Facility.Core;
using Facility.Core.Http;
using Facility.ExampleApi.Http;
using Facility.ExampleApi.InMemory;
using Newtonsoft.Json.Linq;
using Shouldly;

namespace Facility.ExampleApi.UnitTests
{
	internal static class TestUtility
	{
		public static void ShouldBeEquivalent(this ServiceDto actual, ServiceDto expected)
		{
			if (!ServiceDataUtility.AreEquivalentDtos(actual, expected))
			{
				var actualString = actual.ToString();
				actualString.ShouldBe(expected.ToString());
				throw new InvalidOperationException("Non-equivalent objects have same string representation: " + actualString);
			}
		}

		public static void ShouldBeEquivalent(this ServiceResult actual, ServiceResult expected)
		{
			if (!ServiceDataUtility.AreEquivalentResults(actual, expected))
			{
				var actualString = actual.ToString();
				actualString.ShouldBe(expected.ToString());
				throw new InvalidOperationException("Non-equivalent objects have same string representation: " + actualString);
			}
		}

		public static void ShouldBeEquivalent(this JObject actual, JObject expected)
		{
			if (!ServiceDataUtility.AreEquivalentObjects(actual, expected))
			{
				var actualString = actual.ToString();
				actualString.ShouldBe(expected.ToString());
				throw new InvalidOperationException("Non-equivalent objects have same string representation: " + actualString);
			}
		}

		public static void ShouldBeEquivalent(this byte[] actual, byte[] expected)
		{
			if (!ServiceDataUtility.AreEquivalentBytes(actual, expected))
			{
				var actualString = BitConverter.ToString(actual);
				actualString.ShouldBe(BitConverter.ToString(expected));
				throw new InvalidOperationException("Non-equivalent objects have same string representation: " + actualString);
			}
		}

		public static void ShouldBeSuccess(this ServiceResult actual)
		{
			actual.IsSuccess.ShouldBe(true);
		}

		public static void ShouldBeSuccess<T>(this ServiceResult<T> actual, T expected)
			where T : ServiceDto
		{
			actual.Value.ShouldBeEquivalent(expected);
		}

		public static void ShouldBeFailure(this ServiceResult actual)
		{
			actual.IsFailure.ShouldBe(true);
		}

		public static void ShouldBeFailure(this ServiceResult actual, string expected)
		{
			actual.IsFailure.ShouldBe(true);
			actual.Error.Code.ShouldBe(expected);
		}

		public static void ShouldBeFailure(this ServiceResult actual, ServiceErrorDto expected)
		{
			actual.IsFailure.ShouldBe(true);
			actual.Error.ShouldBeEquivalent(expected);
		}

		public static IExampleApi CreateService(string category)
		{
			switch (category)
			{
			case "InMemory":
				return CreateInMemoryService();
			case "TestHttpClient":
				return new HttpClientExampleApi(new HttpClientServiceSettings { HttpClient = CreateTestHttpClient() });
			default:
				throw new ArgumentOutOfRangeException(nameof(category));
			}
		}

		public static HttpClient CreateTestHttpClient()
		{
			var service = CreateInMemoryService();
			var handler = new ExampleApiHttpHandler(service, new ServiceHttpHandlerSettings { RootPath = "/v1" });
			return new HttpClient(handler);
		}

		private static ExampleApiService CreateInMemoryService()
		{
			return new ExampleApiService(InMemoryExampleApiRepository.CreateWithSampleWidgets());
		}
	}
}
