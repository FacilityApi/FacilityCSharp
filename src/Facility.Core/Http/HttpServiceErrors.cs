using System.Globalization;
using System.Net;
using System.Net.Http;

namespace Facility.Core.Http
{
	/// <summary>
	/// Common service errors.
	/// </summary>
	public static partial class HttpServiceErrors
	{
		/// <summary>
		/// Creates an error DTO for the specified status code.
		/// </summary>
		public static ServiceErrorDto CreateErrorForStatusCode(HttpStatusCode statusCode)
		{
			return CreateErrorForStatusCode(statusCode, null);
		}

		/// <summary>
		/// An invalid request/response for missing Content-Type.
		/// </summary>
		public static ServiceErrorDto CreateMissingContentType(string errorCode)
		{
			return new ServiceErrorDto(errorCode, "HTTP content missing Content-Type.");
		}

		/// <summary>
		/// An invalid request/response for unsupported Content-Type.
		/// </summary>
		public static ServiceErrorDto CreateUnsupportedContentType(string errorCode, string contentType)
		{
			return new ServiceErrorDto(errorCode, $"HTTP content has unsupported Content-Type: {contentType}");
		}

		/// <summary>
		/// An invalid request/response for bad content.
		/// </summary>
		public static ServiceErrorDto CreateInvalidContent(string errorCode, string message)
		{
			return new ServiceErrorDto(errorCode, $"HTTP content is invalid: {message}");
		}

		/// <summary>
		/// The HTTP request header has an invalid format.
		/// </summary>
		public static ServiceErrorDto CreateRequestHeaderInvalidFormat(string headerName)
		{
			return ServiceErrors.CreateInvalidRequest($"HTTP request header '{headerName}' has an invalid format.");
		}

		/// <summary>
		/// The HTTP request header is not supported.
		/// </summary>
		public static ServiceErrorDto CreateRequestHeaderNotSupported(string headerName)
		{
			return ServiceErrors.CreateInvalidRequest($"HTTP request header '{headerName}' is not supported.");
		}

		internal static ServiceErrorDto CreateErrorForStatusCode(HttpStatusCode statusCode, string reasonPhrase)
		{
			int statusCodeNumber = (int) statusCode;
			bool isClientError = statusCodeNumber >= 400 && statusCodeNumber <= 499;
			bool isServerError = statusCodeNumber >= 500 && statusCodeNumber <= 599;
			string errorCode = TryGetErrorCode(statusCode) ?? (isClientError ? ServiceErrors.InvalidRequest : ServiceErrors.InvalidResponse);
			string message = isServerError ? "HTTP server error" : isClientError ? "HTTP client error" : "Unexpected HTTP status code";
			string statusCodeString = statusCodeNumber.ToString(CultureInfo.InvariantCulture);
			if (string.IsNullOrEmpty(reasonPhrase))
				reasonPhrase = new HttpResponseMessage(statusCode).ReasonPhrase;
			return new ServiceErrorDto(errorCode, $"{message}: {statusCodeString} {reasonPhrase}");
		}

		internal static ServiceErrorDto CreateResponseHeaderInvalidFormat(string headerName)
		{
			return ServiceErrors.CreateInvalidResponse($"HTTP response header '{headerName}' has an invalid format.");
		}

		internal static ServiceErrorDto CreateResponseHeaderNotSupported(string headerName)
		{
			return ServiceErrors.CreateInvalidResponse($"HTTP response header '{headerName}' is not supported.");
		}
	}
}
