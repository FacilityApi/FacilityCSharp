using System.Globalization;
using System.Net;

namespace Facility.Core.Http;

/// <summary>
/// Common service errors.
/// </summary>
public static partial class HttpServiceErrors
{
	/// <summary>
	/// Creates an error DTO for the specified status code.
	/// </summary>
	public static ServiceErrorDto CreateErrorForStatusCode(HttpStatusCode statusCode) =>
		CreateErrorForStatusCode(statusCode, null);

	/// <summary>
	/// HTTP content missing Content-Type.
	/// </summary>
	public static ServiceErrorDto CreateMissingContentType() =>
		ServiceErrors.CreateInvalidRequest("HTTP content missing Content-Type.");

	/// <summary>
	/// HTTP content has unsupported Content-Type.
	/// </summary>
	public static ServiceErrorDto CreateUnsupportedContentType(string contentType) =>
		ServiceErrors.CreateInvalidRequest($"HTTP content has unsupported Content-Type: {contentType}");

	/// <summary>
	/// HTTP content is invalid.
	/// </summary>
	public static ServiceErrorDto CreateInvalidContent(string message) =>
		ServiceErrors.CreateInvalidRequest($"HTTP content is invalid: {message}");

	/// <summary>
	/// HTTP header has an invalid format.
	/// </summary>
	public static ServiceErrorDto CreateHeaderInvalidFormat(string headerName) =>
		ServiceErrors.CreateInvalidRequest($"HTTP header '{headerName}' has an invalid format.");

	/// <summary>
	/// The HTTP header is not supported.
	/// </summary>
	public static ServiceErrorDto CreateHeaderNotSupported(string headerName) =>
		ServiceErrors.CreateInvalidRequest($"HTTP header '{headerName}' is not supported.");

	internal static ServiceErrorDto CreateErrorForStatusCode(HttpStatusCode statusCode, string? reasonPhrase)
	{
		var statusCodeNumber = (int) statusCode;
		var isClientError = statusCodeNumber is >= 400 and <= 499;
		var isServerError = statusCodeNumber is >= 500 and <= 599;
		var errorCode = TryGetErrorCode(statusCode) ?? (isClientError ? ServiceErrors.InvalidRequest : ServiceErrors.InvalidResponse);
		var message = isServerError ? "HTTP server error" : isClientError ? "HTTP client error" : "Unexpected HTTP status code";
		var statusCodeString = statusCodeNumber.ToString(CultureInfo.InvariantCulture);
		if (string.IsNullOrEmpty(reasonPhrase))
			reasonPhrase = new HttpResponseMessage(statusCode).ReasonPhrase;
		return new ServiceErrorDto(errorCode, $"{message}: {statusCodeString} {reasonPhrase}");
	}
}
