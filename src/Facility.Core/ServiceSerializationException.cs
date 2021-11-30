namespace Facility.Core;

public class ServiceSerializationException : Exception
{
	public ServiceSerializationException(string message)
		: base(message)
	{
	}

	public ServiceSerializationException(Exception innerException)
		: base(innerException.Message, innerException)
	{
	}
}
