using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;

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
		m_forceAsyncIO = settings?.ForceAsyncIO ?? false;
		m_memoryStreamCreator = settings?.MemoryStreamCreator;

		SupportedMediaTypes = [HttpServiceUtility.JsonMediaType];
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
	protected override HttpContent CreateHttpContentCore(object content, string? mediaType)
	{
		var memoryStream = CreateMemoryStream();
		ServiceJsonUtility.ToJsonStream(content, memoryStream);
		return new DelegateHttpContent(mediaType ?? DefaultMediaType, memoryStream);
	}

	/// <summary>
	/// Reads a DTO from the specified HTTP content.
	/// </summary>
	protected override async Task<ServiceResult<object>> ReadHttpContentAsyncCore(Type objectType, HttpContent content, CancellationToken cancellationToken)
	{
		try
		{
			if (m_forceAsyncIO)
			{
				// read content into memory so that ASP.NET Core doesn't complain about synchronous I/O during JSON deserialization
				using var stream = CreateMemoryStream();
#if !NETSTANDARD2_0
				await content.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
#else
				await content.CopyToAsync(stream).ConfigureAwait(false);
#endif
				stream.Seek(0, SeekOrigin.Begin);
				return ReadJsonStream(objectType, stream);
			}
			else
			{
#if !NETSTANDARD2_0
				using var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
				using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
				return ReadJsonStream(objectType, stream);
			}
		}
		catch (JsonException exception)
		{
			return ServiceResult.Failure(HttpServiceErrors.CreateInvalidContent(exception.Message));
		}
	}

	private static ServiceResult<object> ReadJsonStream(Type objectType, Stream stream)
	{
		using var textReader = new StreamReader(stream);
		var deserializedContent = ServiceJsonUtility.FromJsonTextReader(textReader, objectType);
		if (deserializedContent is null)
			return ServiceResult.Failure(HttpServiceErrors.CreateInvalidContent("Content must not be empty."));
		return ServiceResult.Success(deserializedContent);
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

	private readonly bool m_forceAsyncIO;
	private readonly Func<Stream>? m_memoryStreamCreator;
}
