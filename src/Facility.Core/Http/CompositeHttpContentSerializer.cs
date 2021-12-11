namespace Facility.Core.Http;

internal sealed class CompositeHttpContentSerializer : HttpContentSerializer
{
	public CompositeHttpContentSerializer(IReadOnlyList<HttpContentSerializer> contentSerializers)
	{
		m_contentSerializers = contentSerializers ?? throw new ArgumentNullException(nameof(contentSerializers));
		if (m_contentSerializers.Count <= 1)
			throw new ArgumentException("At least two content serializers must be specified.", nameof(contentSerializers));
	}

	protected override string DefaultMediaTypeCore => m_contentSerializers[0].DefaultMediaType;

	protected override bool IsSupportedMediaTypeCore(string mediaType) => m_contentSerializers.Any(x => x.IsSupportedMediaType(mediaType));

	protected override bool IsAcceptedMediaTypeCore(string mediaType) => m_contentSerializers.Any(x => x.IsAcceptedMediaType(mediaType));

	protected override HttpContent CreateHttpContentCore(object content, string? mediaType)
	{
		var contentSerializer = m_contentSerializers.FirstOrDefault(x => x.IsAcceptedMediaType(mediaType ?? "")) ??
			m_contentSerializers.FirstOrDefault(x => x.IsSupportedMediaType(mediaType ?? "")) ??
			m_contentSerializers[0];

		return contentSerializer.CreateHttpContent(content, mediaType);
	}

	protected override async Task<ServiceResult<object>> ReadHttpContentAsyncCore(Type objectType, HttpContent content, CancellationToken cancellationToken)
	{
		var contentType = content.Headers.ContentType?.MediaType ?? "";
		var contentSerializer = m_contentSerializers.FirstOrDefault(x => x.IsSupportedMediaType(contentType)) ?? m_contentSerializers[0];

		return await contentSerializer.ReadHttpContentAsync(objectType, content, cancellationToken).ConfigureAwait(false);
	}

	private readonly IReadOnlyList<HttpContentSerializer> m_contentSerializers;
}
