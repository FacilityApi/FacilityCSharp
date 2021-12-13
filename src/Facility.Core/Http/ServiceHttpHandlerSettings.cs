namespace Facility.Core.Http;

/// <summary>
/// Settings for service HTTP handlers.
/// </summary>
public class ServiceHttpHandlerSettings
{
	/// <summary>
	/// The root path of the service, default "/".
	/// </summary>
	public string? RootPath { get; set; }

	/// <summary>
	/// True to call services synchronously, allowing tasks to be safely blocked.
	/// </summary>
	public bool Synchronous { get; set; }

	/// <summary>
	/// The content serializer used by requests and responses.
	/// </summary>
	public HttpContentSerializer? ContentSerializer { get; set; }

	/// <summary>
	/// The content serializer used by requests and responses for bytes (optional).
	/// </summary>
	public HttpContentSerializer? BytesSerializer { get; set; }

	/// <summary>
	/// The content serializer used by requests and responses for text (optional).
	/// </summary>
	public HttpContentSerializer? TextSerializer { get; set; }

	/// <summary>
	/// True to disable chunked transfer encoding (default false).
	/// </summary>
	/// <remarks>If true, the response is serialized into memory before sending it to the client.</remarks>
	public bool DisableChunkedTransfer { get; set; }

	/// <summary>
	/// The aspects used when receiving requests and sending responses.
	/// </summary>
	public IReadOnlyList<ServiceHttpHandlerAspect>? Aspects { get; set; }

	/// <summary>
	/// True to prevent the validation of request DTOs after they are received.
	/// </summary>
	public bool SkipRequestValidation { get; set; }

	/// <summary>
	/// True to prevent the validation of response DTOs before they are sent.
	/// </summary>
	public bool SkipResponseValidation { get; set; }
}
