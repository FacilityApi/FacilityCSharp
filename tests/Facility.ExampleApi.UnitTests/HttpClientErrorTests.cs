using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Facility.Core;
using Facility.Core.Assertions;
using Facility.Core.Http;
using Facility.ExampleApi.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.ExampleApi.UnitTests
{
	[SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait", Justification = "unit tests")]
	public class HttpClientErrorTests
	{
		[Test]
		public async Task ServiceUnavailableJsonClientError()
		{
			var error = ServiceErrors.CreateServiceUnavailable();

			var service = CreateTestService(request =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
				response.Content = new StringContent($"{{\"code\":\"{error.Code}\",\"message\":\"{error.Message}\"}}");
				response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(HttpServiceUtility.JsonMediaType);
				return response;
			});

			var result = await service.CreateWidgetAsync(new WidgetDto(name: "hi"));
			result.Error.Should().BeDto(error);
		}

		[Test]
		public async Task BadGatewayHtmlClientError()
		{
			var service = CreateTestService(request =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.BadGateway);
				response.Content = new StringContent("<html><body><h1>Bad Gateway!</h1></body></html>");
				response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");
				return response;
			});

			var result = await service.CreateWidgetAsync(new WidgetDto(name: "hi"));
			result.Error.Should().BeDto(HttpServiceErrors.CreateErrorForStatusCode(HttpStatusCode.BadGateway));
		}

		[Test]
		public async Task NotFoundHtmlClientError()
		{
			var service = CreateTestService(request =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.NotFound);
				response.Content = new StringContent("<html><body><h1>Not Found!</h1></body></html>");
				response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");
				return response;
			});

			var result = await service.CreateWidgetAsync(new WidgetDto(name: "hi"));
			result.Error.Should().BeDto(HttpServiceErrors.CreateErrorForStatusCode(HttpStatusCode.NotFound));
		}

		[Test]
		public async Task BrokenJsonFromServer()
		{
			var service = CreateTestService(request =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.Created);
				response.Content = new StringContent("{");
				response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(HttpServiceUtility.JsonMediaType);
				return response;
			});

			var result = await service.CreateWidgetAsync(new WidgetDto(name: "hi"));
			result.Error.Code.Should().Be(ServiceErrors.InvalidResponse);
		}

		[Test]
		public async Task UnexpectedJsonFromServer()
		{
			var service = CreateTestService(request =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.Created);
				response.Content = new StringContent("{\"name\": {}}");
				response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(HttpServiceUtility.JsonMediaType);
				return response;
			});

			var result = await service.CreateWidgetAsync(new WidgetDto(name: "hi"));
			result.Error.Code.Should().Be(ServiceErrors.InvalidResponse);
		}

		private static IExampleApi CreateTestService(Func<HttpRequestMessage, HttpResponseMessage> sendAsync)
		{
			return new HttpClientExampleApi(new HttpClientServiceSettings { HttpClient = new HttpClient(new OurHttpMessageHandler(sendAsync)) });
		}

		private sealed class OurHttpMessageHandler : HttpMessageHandler
		{
			public OurHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> send)
			{
				m_send = send;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return Task.FromResult(m_send(request));
			}

			readonly Func<HttpRequestMessage, HttpResponseMessage> m_send;
		}
	}
}
