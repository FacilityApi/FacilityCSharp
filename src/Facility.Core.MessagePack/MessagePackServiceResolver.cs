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

			if (type == typeof(DateTime))
				return (IMessagePackFormatter<T>) (object) DateTimeMessagePackFormatter.Instance;
			if (type == typeof(DateTime?))
				return (IMessagePackFormatter<T>) (object) NullableDateTimeMessagePackFormatter.Instance;

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

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ServiceNullable<>))
				return (IMessagePackFormatter<T>) Activator.CreateInstance(typeof(ServiceNullableMessagePackFormatter<>).MakeGenericType(typeof(T).GetGenericArguments()[0]))!;

			if (StandardResolver.Instance.GetFormatter<T>() is { } standardFormatter)
				return standardFormatter;

			var supportedInterface = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));
			if (supportedInterface is not null)
				return (IMessagePackFormatter<T>) Activator.CreateInstance(typeof(CastToSerializeMessagePackFormatter<,>).MakeGenericType(typeof(T), supportedInterface))!;

			throw new InvalidOperationException($"Facility.Core.MessagePack cannot serialize {typeof(T).Name}.");
		}
	}

	private static DateTime TruncateDateTime(DateTime value) =>
		new(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);

	private sealed class DateTimeMessagePackFormatter : IMessagePackFormatter<DateTime>
	{
		public static readonly DateTimeMessagePackFormatter Instance = new();

		public void Serialize(ref MessagePackWriter writer, DateTime value, MessagePackSerializerOptions options) =>
			writer.Write(TruncateDateTime(value));

		public DateTime Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
			reader.ReadDateTime();
	}

	private sealed class NullableDateTimeMessagePackFormatter : IMessagePackFormatter<DateTime?>
	{
		public static readonly NullableDateTimeMessagePackFormatter Instance = new();

		public void Serialize(ref MessagePackWriter writer, DateTime? value, MessagePackSerializerOptions options)
		{
			if (value is null)
			{
				writer.WriteNil();
				return;
			}

			var dateTimeValue = value.Value;

			if (dateTimeValue.Kind != DateTimeKind.Utc)
				throw new MessagePackSerializationException("DateTime must use DateTimeKind.Utc.");

			writer.Write(TruncateDateTime(dateTimeValue));
		}

		public DateTime? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
			reader.TryReadNil() ? null : reader.ReadDateTime();
	}

	private sealed class CastToSerializeMessagePackFormatter<T, TCast> : IMessagePackFormatter<T>
	{
		public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options) =>
			Cache<TCast>.Formatter.Serialize(ref writer, (TCast) (object) value!, options);

		public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
			throw new InvalidOperationException($"Facility.Core.MessagePack cannot deserialize {typeof(T).Name}.");
	}
}
