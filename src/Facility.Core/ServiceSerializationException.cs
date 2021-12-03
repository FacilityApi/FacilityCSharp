namespace Facility.Core;

/// <summary>
/// Thrown when JSON deserialization fails.
/// </summary>
public class ServiceSerializationException : Exception
{
	/// <summary>
	/// Creates an instance.
	/// </summary>
	public ServiceSerializationException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Creates an instance.
	/// </summary>
	public ServiceSerializationException(Exception innerException)
		: base(innerException.Message, innerException)
	{
	}
}
