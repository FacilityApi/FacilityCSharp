namespace Facility.Core
{
	/// <summary>
	/// Common service errors.
	/// </summary>
	public static partial class ServiceErrors
	{
		/// <summary>
		/// The specified request field is required.
		/// </summary>
		public static ServiceErrorDto CreateRequestFieldRequired(string fieldName)
		{
			return CreateInvalidRequest($"The request field '{fieldName}' is required.");
		}

		/// <summary>
		/// The HTTP request header has an invalid format.
		/// </summary>
		public static ServiceErrorDto CreateRequestHeaderInvalidFormat(string headerName)
		{
			return CreateInvalidRequest($"HTTP request header '{headerName}' has an invalid format.");
		}

		/// <summary>
		/// The HTTP response header has an invalid format.
		/// </summary>
		public static ServiceErrorDto CreateResponseHeaderInvalidFormat(string headerName)
		{
			return CreateInvalidRequest($"HTTP response header '{headerName}' has an invalid format.");
		}

		/// <summary>
		/// The HTTP request header is not supported.
		/// </summary>
		public static ServiceErrorDto CreateRequestHeaderNotSupported(string headerName)
		{
			return CreateInvalidRequest($"HTTP request header '{headerName}' is not supported.");
		}

		/// <summary>
		/// The HTTP response header is not supported.
		/// </summary>
		public static ServiceErrorDto CreateResponseHeaderNotSupported(string headerName)
		{
			return CreateInvalidRequest($"HTTP response header '{headerName}' is not supported.");
		}
	}
}
