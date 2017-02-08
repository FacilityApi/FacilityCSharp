using System;
using System.Net.Http;
using Facility.Core.Http;
using Facility.ExampleApi.Http;
using Facility.ExampleApi.InMemory;

namespace Facility.ExampleApi.UnitTests
{
	internal static class TestUtility
	{
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
