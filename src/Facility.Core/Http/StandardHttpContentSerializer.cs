using System.Net;
using System.Net.Http.Headers;

namespace Facility.Core.Http;

internal sealed class StandardHttpContentSerializer : HttpContentSerializer
{
	public StandardHttpContentSerializer(ServiceSerializer serializer)
	{
		m_serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
	}

	protected override string DefaultMediaTypeCore => m_serializer.DefaultMediaType;

	protected override bool IsSupportedMediaTypeCore(string mediaType) => mediaType == m_serializer.DefaultMediaType;

	protected override HttpContent CreateHttpContentCore(object content, string? mediaType)
	{
		var memoryStream = new MemoryStream();
		m_serializer.ToStream(content, memoryStream);
		return new DelegateHttpContent(mediaType ?? DefaultMediaType, memoryStream);
	}

	/// <summary>
	/// Reads a DTO from the specified HTTP content.
	/// </summary>
	protected override async Task<ServiceResult<object>> ReadHttpContentAsyncCore(Type objectType, HttpContent content, CancellationToken cancellationToken)
	{
		try
		{
#if NET6_0_OR_GREATER
			using var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
			using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
			using var memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream, 80 * 1024, cancellationToken).ConfigureAwait(false);
			memoryStream.Seek(0, SeekOrigin.Begin);
			var deserializedContent = m_serializer.FromStream(memoryStream, objectType);
			if (deserializedContent is null)
				return ServiceResult.Failure(HttpServiceErrors.CreateInvalidContent("Content must not be empty."));
			return ServiceResult.Success(deserializedContent);
		}
		catch (ServiceSerializationException exception)
		{
			return ServiceResult.Failure(HttpServiceErrors.CreateInvalidContent(exception.Message));
		}
	}

	private sealed class DelegateHttpContent : HttpContent
	{
		public DelegateHttpContent(string mediaType, Stream memoryStream)
		{
			Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

			m_memoryStream = memoryStream;
		}

		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
		{
			m_memoryStream.Position = 0;
			return m_memoryStream.CopyToAsync(stream);
		}

		protected override bool TryComputeLength(out long length)
		{
			length = m_memoryStream.Length;
			return true;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
					m_memoryStream.Dispose();
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		private readonly Stream m_memoryStream;
	}

	private readonly ServiceSerializer m_serializer;
}
