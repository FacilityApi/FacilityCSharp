using Newtonsoft.Json.Linq;

namespace Facility.Core
{
	/// <summary>
	/// Helper methods for service errors.
	/// </summary>
	public static class ServiceErrorUtility
	{
		/// <summary>
		/// Creates an InternalError service error for an exception.
		/// </summary>
		public static ServiceErrorDto CreateInternalErrorForException(Exception exception)
		{
			var error = ServiceErrors.CreateInternalError(exception.Message);
			var jException = new JObject { ["details"] = new JArray(exception.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()) };
			error.DetailsObject = ServiceObject.Create(new JObject { ["exception"] = jException });
			return error;
		}
	}
}
