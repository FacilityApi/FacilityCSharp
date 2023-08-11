using MessagePack;
using MessagePack.Formatters;

namespace Facility.Core.MessagePack;

internal sealed class DateTimeMessagePackFormatter : IMessagePackFormatter<DateTime>
{
	public static readonly DateTimeMessagePackFormatter Instance = new();

	public void Serialize(ref MessagePackWriter writer, DateTime value, MessagePackSerializerOptions options) =>
		writer.Write(new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind));

	public DateTime Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
		reader.ReadDateTime();
}
