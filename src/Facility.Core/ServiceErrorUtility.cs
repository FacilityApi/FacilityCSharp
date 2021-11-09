using System;
using System.Collections.Generic;

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
			var exceptionInfo = new Dictionary<string, object?> { ["details"] = exception.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) };
			error.Details = new Dictionary<string, object?> { ["exception"] = exceptionInfo };
			return error;
		}
	}
}
