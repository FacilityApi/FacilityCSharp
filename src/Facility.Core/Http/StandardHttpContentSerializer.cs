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

	protected override HttpContent CreateHttpContentCore(object content, string? mediaType) =>
		new DelegateHttpContent(mediaType ?? DefaultMediaType, content, m_serializer);

	protected override async Task<ServiceResult<object>> ReadHttpContentAsyncCore(Type objectType, HttpContent content, CancellationToken cancellationToken)
	{
		try
		{
#if NET6_0_OR_GREATER
			var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
			await using var streamScope = stream.ConfigureAwait(false);
#elif NETSTANDARD2_1_OR_GREATER
			var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
			await using var streamScope = stream.ConfigureAwait(false);
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
		public DelegateHttpContent(string mediaType, object content, ServiceSerializer serializer)
		{
			Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

			m_content = content;
			m_serializer = serializer;
		}

		protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
			await m_serializer.ToStreamAsync(m_content, stream, CancellationToken.None).ConfigureAwait(false);

		protected override bool TryComputeLength(out long length)
		{
			length = -1L;
			return false;
		}

		private readonly object m_content;
		private readonly ServiceSerializer m_serializer;
	}

	private readonly ServiceSerializer m_serializer;
}
