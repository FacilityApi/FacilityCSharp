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
		/// HTTP content missing Content-Type.
		/// </summary>
		public static ServiceErrorDto CreateMissingContentType()
		{
			return ServiceErrors.CreateInvalidRequest("HTTP content missing Content-Type.");
		}

		/// <summary>
		/// HTTP content has unsupported Content-Type.
		/// </summary>
		public static ServiceErrorDto CreateUnsupportedContentType(string contentType)
		{
			return ServiceErrors.CreateInvalidRequest($"HTTP content has unsupported Content-Type: {contentType}");
		}

		/// <summary>
		/// HTTP content is invalid.
		/// </summary>
		public static ServiceErrorDto CreateInvalidContent(string message)
		{
			return ServiceErrors.CreateInvalidRequest($"HTTP content is invalid: {message}");
		}

		/// <summary>
		/// HTTP header has an invalid format.
		/// </summary>
		public static ServiceErrorDto CreateHeaderInvalidFormat(string headerName)
		{
			return ServiceErrors.CreateInvalidRequest($"HTTP header '{headerName}' has an invalid format.");
		}

		/// <summary>
		/// The HTTP header is not supported.
		/// </summary>
		public static ServiceErrorDto CreateHeaderNotSupported(string headerName)
		{
			return ServiceErrors.CreateInvalidRequest($"HTTP header '{headerName}' is not supported.");
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
	}
}
