using System.Net;
#if NET472
using System.Net.Http;
#endif
using System.Net.Http.Headers;
using System.Text;
using Facility.ConformanceApi;
using Facility.ConformanceApi.Http;
using Facility.Core.Assertions;
using Facility.Core.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Core.UnitTests.Http;

internal sealed class InvalidResponseTests
{
	[Test]
	public async Task MissingContent()
	{
		// missing content isn't actually invalid, but rather treated as an empty request
		(await GetApiInfoResult(_ => new HttpResponseMessage(HttpStatusCode.OK))).Should().BeSuccess(new());
		(await GetApiInfoResult(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([]) })).Should().BeSuccess(new());
		(await GetApiInfoResult(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new NoLengthContent([]) })).Should().BeSuccess(new());
	}

	[Test]
	public async Task ValidContent()
	{
		// check with Content-Length header and without
		var contentWithLength = new ByteArrayContent([(byte) '{', (byte) '}']) { Headers = { ContentType = MediaTypeHeaderValue.Parse("application/json") } };
		(await GetApiInfoResult(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = contentWithLength })).Should().BeSuccess(new());
		var contentWithoutLength = new NoLengthContent([(byte) '{', (byte) '}']) { Headers = { ContentType = MediaTypeHeaderValue.Parse("application/json") } };
		(await GetApiInfoResult(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = contentWithoutLength })).Should().BeSuccess(new());
	}

	[Test]
	public async Task MissingContentType()
	{
		await GetApiInfoInvalidResponse(
			_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([(byte) '{', (byte) '}']) },
			ServiceErrors.InvalidResponse, HttpServiceErrors.CreateMissingContentType().Message);

		await GetApiInfoInvalidResponse(
			_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new NoLengthContent([(byte) '{', (byte) '}']) },
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
			_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("", Encoding.UTF8, "application/json") },
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

	private async Task<ServiceResult<GetApiInfoResponseDto>> GetApiInfoResult(Func<HttpRequestMessage, HttpResponseMessage> send)
	{
		var handler = new FakeHttpHandler(send);
		var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://example.com/") };
		var clientApi = new HttpClientConformanceApi(new HttpClientServiceSettings { HttpClient = httpClient });
		return await clientApi.GetApiInfoAsync(new GetApiInfoRequestDto());
	}

	private async Task GetApiInfoInvalidResponse(Func<HttpRequestMessage, HttpResponseMessage> send, string code, string? message)
	{
		var result = await GetApiInfoResult(send);
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

	private sealed class NoLengthContent(byte[] bytes) : HttpContent
	{
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => stream.WriteAsync(bytes, 0, bytes.Length);

		protected override bool TryComputeLength(out long length)
		{
			length = 0;
			return false;
		}
	}
}
