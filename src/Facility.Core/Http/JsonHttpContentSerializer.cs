namespace Facility.Core.Http;

/// <summary>
/// Uses JSON to serialize and deserialize DTOs for HTTP requests and responses.
/// </summary>
[Obsolete("Use HttpContentSerializer.Create(SystemTextJsonServiceSerializer.Instance).")]
public class JsonHttpContentSerializer : HttpContentSerializer
{
	/// <summary>
	/// An instance of JsonHttpContentSerializer.
	/// </summary>
	public static readonly JsonHttpContentSerializer Instance = new();

	/// <summary>
	/// Creates an instance.
	/// </summary>
	public JsonHttpContentSerializer()
		: this(settings: null)
	{
	}

	/// <summary>
	/// Creates an instance.
	/// </summary>
	public JsonHttpContentSerializer(JsonHttpContentSerializerSettings? settings)
	{
		// we always force async I/O now, so we can ignore settings.ForceAsyncIO
		m_memoryStreamCreator = settings?.MemoryStreamCreator;
		m_contentSerializer = Legacy.WithMemoryStreamCreatorIfNotNull(m_memoryStreamCreator);

		SupportedMediaTypes = new[] { HttpServiceUtility.JsonMediaType };
	}

	/// <summary>
	/// The supported media types. Defaults to "application/json".
	/// </summary>
	public IReadOnlyList<string> SupportedMediaTypes { get; }

	/// <summary>
	/// Creates a memory stream.
	/// </summary>
	protected virtual Stream CreateMemoryStream() => m_memoryStreamCreator is null ? new MemoryStream() : m_memoryStreamCreator();

	/// <summary>
	/// The media type for requests.
	/// </summary>
	protected override string DefaultMediaTypeCore => SupportedMediaTypes[0];

	/// <summary>
	/// Determines if the specified media type is supported.
	/// </summary>
	protected override bool IsSupportedMediaTypeCore(string mediaType) => SupportedMediaTypes.Contains(mediaType);

	/// <summary>
	/// Creates HTTP content for the specified DTO.
	/// </summary>
	protected override HttpContent CreateHttpContentCore(object content, string? mediaType) =>
		m_contentSerializer.CreateHttpContent(content, mediaType ?? DefaultMediaType);

	/// <summary>
	/// Reads a DTO from the specified HTTP content.
	/// </summary>
	protected override Task<ServiceResult<object>> ReadHttpContentAsyncCore(Type objectType, HttpContent content, CancellationToken cancellationToken) =>
		m_contentSerializer.ReadHttpContentAsync(objectType, content, cancellationToken);

	private readonly Func<Stream>? m_memoryStreamCreator;
	private readonly HttpContentSerializer m_contentSerializer;
}
