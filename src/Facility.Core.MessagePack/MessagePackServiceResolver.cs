using MessagePack;
using MessagePack.Formatters;

namespace Facility.Core.MessagePack;

internal sealed class MessagePackServiceResolver : IFormatterResolver
{
	public static readonly MessagePackServiceResolver Instance = new();

	public IMessagePackFormatter<T>? GetFormatter<T>()
	{
		var type = typeof(T);

		if (type == typeof(ServiceObject))
			return (IMessagePackFormatter<T>) (object) ServiceObjectMessagePackFormatter.Instance;

		if (type == typeof(ServiceErrorDto))
			return (IMessagePackFormatter<T>) (object) ServiceErrorMessagePackFormatter.Instance;

		if (type == typeof(ServiceResult))
			return (IMessagePackFormatter<T>) (object) ServiceResultMessagePackFormatter.Instance;

		if (type == typeof(ServiceResultFailure))
			return (IMessagePackFormatter<T>) (object) ServiceResultFailureMessagePackFormatter.Instance;

		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ServiceResult<>))
			return ServiceResultFormatterCache<T>.Formatter;

		return null;
	}

	private MessagePackServiceResolver()
	{
	}

	private static class ServiceResultFormatterCache<T>
	{
		public static readonly IMessagePackFormatter<T> Formatter =
			(IMessagePackFormatter<T>) Activator.CreateInstance(typeof(ServiceResultMessagePackFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]))!;
	}
}
