namespace Facility.Core.Http;

/// <summary>
/// Serializes and deserializes values for HTTP requests and responses.
/// </summary>
public abstract class HttpContentSerializer
{
	/// <summary>
	/// Creates a standard HTTP content serializer.
	/// </summary>
	/// <remarks>Values are serialized and deserialized via memory streams so that the actual I/O can be asynchronous
	/// whether or not the serialization format supports asynchronous I/O directly. This also makes it simple to calculate
	/// the length of the content. Consider using <c>Microsoft.IO.RecyclableMemoryStream</c> to improve performance
	/// by setting <c>memoryStreamCreator</c> to <c>RecyclableMemoryStreamManager.GetStream</c>. Otherwise
	/// <c>System.IO.MemoryStream</c> is used.</remarks>
	public static HttpContentSerializer Create(ServiceSerializer serviceSerializer, Func<Stream>? memoryStreamCreator = null) =>
		new StandardHttpContentSerializer(serviceSerializer, memoryStreamCreator);

	/// <summary>
	/// The default media type for the serializer.
	/// </summary>
	public string DefaultMediaType => DefaultMediaTypeCore;

	/// <summary>
	/// Determines if the specified media type can be read by this serializer.
	/// </summary>
	public bool IsSupportedMediaType(string mediaType) => IsSupportedMediaTypeCore(mediaType);

	/// <summary>
	/// Determines if the specified media type is accepted when investigating the Accept header.
	/// </summary>
	public bool IsAcceptedMediaType(string mediaType) => IsAcceptedMediaTypeCore(mediaType);

	/// <summary>
	/// Creates HTTP content for the specified DTO.
	/// </summary>
	public HttpContent CreateHttpContent(object content, string? mediaType = null) => CreateHttpContentCore(content, mediaType);

	/// <summary>
	/// Reads a DTO from the specified HTTP content.
	/// </summary>
	public async Task<ServiceResult<T>> ReadHttpContentAsync<T>(HttpContent? content, CancellationToken cancellationToken = default)
		where T : ServiceDto
	{
		return (await ReadHttpContentAsync(typeof(T), content, cancellationToken).ConfigureAwait(false)).Cast<T>();
	}

	/// <summary>
	/// Reads an object from the specified HTTP content.
	/// </summary>
	public async Task<ServiceResult<object>> ReadHttpContentAsync(Type objectType, HttpContent? content, CancellationToken cancellationToken = default)
	{
		var contentType = content?.Headers.ContentType;
		if (contentType == null)
			return ServiceResult.Failure(HttpServiceErrors.CreateMissingContentType());

		var mediaType = contentType.MediaType ?? "";
		if (!IsSupportedMediaType(mediaType))
			return ServiceResult.Failure(HttpServiceErrors.CreateUnsupportedContentType(mediaType));

		return await ReadHttpContentAsyncCore(objectType, content!, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// The default media type for the serializer.
	/// </summary>
	protected abstract string DefaultMediaTypeCore { get; }

	/// <summary>
	/// Determines if the specified media type can be read by this serializer.
	/// </summary>
	protected abstract bool IsSupportedMediaTypeCore(string mediaType);

	/// <summary>
	/// Determines if the specified media type is accepted when investigating the Accept header.
	/// </summary>
	/// <remarks>Calls <see cref="IsSupportedMediaType"/> by default.</remarks>
	protected virtual bool IsAcceptedMediaTypeCore(string mediaType) => IsSupportedMediaType(mediaType);

	/// <summary>
	/// Creates HTTP content for the specified DTO.
	/// </summary>
	protected abstract HttpContent CreateHttpContentCore(object content, string? mediaType);

	/// <summary>
	/// Reads a DTO from the specified HTTP content.
	/// </summary>
	protected abstract Task<ServiceResult<object>> ReadHttpContentAsyncCore(Type objectType, HttpContent content, CancellationToken cancellationToken);

	internal static HttpContentSerializer Legacy { get; } = Create(NewtonsoftJsonServiceSerializer.Instance);
}
