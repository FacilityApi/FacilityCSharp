using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Facility.ConformanceApi;
using Facility.ConformanceApi.Http;
using Facility.Core.Assertions;
using Facility.Core.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests.Http
{
	public class InvalidResponseTests
	{
		[Test]
		public async Task MissingContentType()
		{
			await GetApiInfoInvalidResponse(
				_ => new HttpResponseMessage(HttpStatusCode.OK),
				ServiceErrors.InvalidResponse, HttpServiceErrors.CreateMissingContentType().Message);
		}

		[Test]
		public async Task UnsupportedContentType()
		{
			await GetApiInfoInvalidResponse(
				_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("text", Encoding.UTF8, "text/plain") },
				ServiceErrors.InvalidResponse, HttpServiceErrors.CreateUnsupportedContentType("text/plain").Message);
		}

		[Test]
		public async Task InvalidJson()
		{
			await GetApiInfoInvalidResponse(
				_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{", Encoding.UTF8, "application/json") },
				ServiceErrors.InvalidResponse, HttpServiceErrors.CreateInvalidContent("").Message);
		}

		[Test]
		public async Task EmptyJson()
		{
			await GetApiInfoInvalidResponse(
				_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty, Encoding.UTF8, "application/json") },
				ServiceErrors.InvalidResponse, HttpServiceErrors.CreateInvalidContent("").Message);
		}

		[Test]
		public async Task UnexpectedSuccess()
		{
			await GetApiInfoInvalidResponse(
				_ => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{}", Encoding.UTF8, "application/json") },
				ServiceErrors.NotFound, HttpServiceErrors.CreateErrorForStatusCode(HttpStatusCode.NotFound).Message);
		}

		[Test]
		public async Task InvalidJsonNotFound()
		{
			await GetApiInfoInvalidResponse(
				_ => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{", Encoding.UTF8, "application/json") },
				ServiceErrors.NotFound, HttpServiceErrors.CreateErrorForStatusCode(HttpStatusCode.NotFound).Message);
		}

		[Test]
		public async Task MissingContentTypeNotFound()
		{
			await GetApiInfoInvalidResponse(
				_ => new HttpResponseMessage(HttpStatusCode.NotFound),
				ServiceErrors.NotFound, HttpServiceErrors.CreateErrorForStatusCode(HttpStatusCode.NotFound).Message);
		}

		[Test]
		public async Task UnsupportedContentTypeNotFound()
		{
			await GetApiInfoInvalidResponse(
				_ => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("text", Encoding.UTF8, "text/plain") },
				ServiceErrors.NotFound, HttpServiceErrors.CreateErrorForStatusCode(HttpStatusCode.NotFound).Message);
		}

		[Test]
		public async Task MissingCodeFailure()
		{
			await GetApiInfoInvalidResponse(
				_ => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("{}", Encoding.UTF8, "application/json") },
				ServiceErrors.NotFound, HttpServiceErrors.CreateErrorForStatusCode(HttpStatusCode.NotFound).Message);
		}

		[Test]
		public async Task ExceptionThrown()
		{
			await GetApiInfoInvalidResponse(
				_ => throw new IOException("boom"),
				ServiceErrors.InternalError, "boom");
		}

		[Test]
		public async Task TimeoutThrown()
		{
			await GetApiInfoInvalidResponse(
				_ => throw new OperationCanceledException(),
				ServiceErrors.Timeout, ServiceErrors.CreateTimeout().Message);
		}

		private async Task GetApiInfoInvalidResponse(Func<HttpRequestMessage, HttpResponseMessage> send, string code, string? message)
		{
			var handler = new FakeHttpHandler(send);
			var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://example.com/") };
			var clientApi = new HttpClientConformanceApi(new HttpClientServiceSettings { HttpClient = httpClient });
			var result = await clientApi.GetApiInfoAsync(new GetApiInfoRequestDto());
			result.Should().BeFailure(code);
			result.Error!.Message.Should().StartWith(message);
		}

		private sealed class FakeHttpHandler : HttpMessageHandler
		{
			public FakeHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> send) => m_send = send;

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
				Task.FromResult(m_send.Invoke(request));

			private readonly Func<HttpRequestMessage, HttpResponseMessage> m_send;
		}
	}
}
