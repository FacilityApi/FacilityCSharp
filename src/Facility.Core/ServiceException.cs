namespace Facility.Core;

/// <summary>
/// An exception thrown by a service method.
/// </summary>
public class ServiceException : Exception
{
	/// <summary>
	/// Creates an exception from an error data object.
	/// </summary>
	public ServiceException(ServiceErrorDto error)
	{
		Error = error ?? throw new ArgumentNullException(nameof(error));
	}

	/// <summary>
	/// Creates an exception from an error data object and an inner exception.
	/// </summary>
	public ServiceException(ServiceErrorDto error, Exception? innerException)
		: base(null, innerException)
	{
		Error = error ?? throw new ArgumentNullException(nameof(error));
	}

	/// <summary>
	/// The error.
	/// </summary>
	public ServiceErrorDto Error { get; }

	/// <summary>
	/// The message.
	/// </summary>
	public override string Message => Error.Message ?? "";

	/// <summary>
	/// The exception type name, full error, inner exception, and stack trace.
	/// </summary>
	public override string ToString()
	{
		var text = GetType().Name + ": " + GetErrorString(Error);

		var innerException = InnerException;
		if (innerException != null)
			text += Environment.NewLine + " ---> " + innerException + Environment.NewLine + "   --- End of inner exception stack trace ---";

		var stackTrace = StackTrace;
		if (stackTrace != null)
			text += Environment.NewLine + stackTrace;

		return text;
	}

	private static string GetErrorString(ServiceErrorDto error, string indent = "")
	{
		var text = error.Message ?? "";

		if (error.Code != null)
			text += " (" + error.Code + ")";

		if (error.DetailsObject != null)
			text += Environment.NewLine + indent + "  Details: " + error.DetailsObject;

		if (error.InnerError != null)
			text += Environment.NewLine + indent + "  InnerError: " + GetErrorString(error.InnerError, indent + "  ");

		return text;
	}
}
