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
		/// Creates an error DTO for the specified status code and reason phrase.
		/// </summary>
		public static ServiceErrorDto CreateErrorForStatusCode(HttpStatusCode statusCode, string reasonPhrase)
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

		/// <summary>
		/// An invalid request/response for missing Content-Type.
		/// </summary>
		public static ServiceErrorDto CreateErrorMissingContentType(string errorCode)
		{
			return new ServiceErrorDto(errorCode, "HTTP content missing Content-Type.");
		}

		/// <summary>
		/// An invalid request/response  for unsupported Content-Type.
		/// </summary>
		public static ServiceErrorDto CreateErrorUnsupportedContentType(string errorCode, string contentType)
		{
			return new ServiceErrorDto(errorCode, $"HTTP content has unsupported Content-Type: {contentType}");
		}

		/// <summary>
		/// An invalid request/response for bad content.
		/// </summary>
		public static ServiceErrorDto CreateErrorBadContent(string errorCode, string message)
		{
			return new ServiceErrorDto(errorCode, $"HTTP content is invalid: {message}");
		}
	}
}
