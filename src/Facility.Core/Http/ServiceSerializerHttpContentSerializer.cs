using System.Net;
using System.Net.Http.Headers;

namespace Facility.Core.Http;

/// <summary>
/// Uses a ServiceSerializer to serialize and deserialize DTOs for HTTP requests and responses.
/// </summary>
public class ServiceSerializerHttpContentSerializer : HttpContentSerializer
{
	/// <summary>
	/// Creates an instance.
	/// </summary>
	public ServiceSerializerHttpContentSerializer(ServiceSerializer serializer)
	{
		m_serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
	}

	protected virtual Stream CreateMemoryStream() => new MemoryStream();

	/// <summary>
	/// The media type for requests.
	/// </summary>
	protected override string DefaultMediaTypeCore => m_serializer.DefaultMediaType;

	/// <summary>
	/// Determines if the specified media type is supported.
	/// </summary>
	protected override bool IsSupportedMediaTypeCore(string mediaType) => m_serializer.IsSupportedMediaType(mediaType);

	/// <summary>
	/// Creates HTTP content for the specified DTO.
	/// </summary>
	protected override HttpContent CreateHttpContentCore(object content, string? mediaType)
	{
		var memoryStream = CreateMemoryStream();
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
			var deserializedContent = await m_serializer.FromStreamAsync(stream, objectType, cancellationToken).ConfigureAwait(false);
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
