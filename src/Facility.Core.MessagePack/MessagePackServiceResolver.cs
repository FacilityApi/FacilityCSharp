using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace Facility.Core.MessagePack;

internal sealed class MessagePackServiceResolver : IFormatterResolver
{
	public static readonly MessagePackServiceResolver Instance = new();

	public IMessagePackFormatter<T> GetFormatter<T>() => Cache<T>.Formatter;

	private MessagePackServiceResolver()
	{
	}

	private static class Cache<T>
	{
		public static readonly IMessagePackFormatter<T> Formatter = GetFormatter();

		private static IMessagePackFormatter<T> GetFormatter()
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
				return (IMessagePackFormatter<T>) Activator.CreateInstance(typeof(ServiceResultMessagePackFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]))!;

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ServiceField<>))
				return (IMessagePackFormatter<T>) Activator.CreateInstance(typeof(ServiceFieldMessagePackFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]))!;

			return StandardResolver.Instance.GetFormatter<T>();
		}
	}
}
