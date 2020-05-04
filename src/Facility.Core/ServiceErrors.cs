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
		public static ServiceErrorDto CreateRequestFieldRequired(string fieldName) =>
			CreateInvalidRequest(ServiceDataUtility.GetRequiredFieldErrorMessage(fieldName));

		/// <summary>
		/// The specified response field is required.
		/// </summary>
		public static ServiceErrorDto CreateResponseFieldRequired(string fieldName) =>
			CreateInvalidResponse(ServiceDataUtility.GetRequiredFieldErrorMessage(fieldName));
	}
}
