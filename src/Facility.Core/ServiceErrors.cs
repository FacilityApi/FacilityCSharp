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
		public static ServiceErrorDto CreateRequestFieldRequired(string fieldName) => CreateInvalidRequest($"The request field '{fieldName}' is required.");
	}
}
