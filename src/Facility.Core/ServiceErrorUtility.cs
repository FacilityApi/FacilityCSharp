using System;
using System.Linq;
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
			error.Details = new JObject { ["exception"] = new JArray(exception.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable()) };
			return error;
		}
	}
}
