using ArgsReading;

namespace fsdgencsharp;

internal static class ArgsReaderExtensions
{
	public static IReadOnlyList<T>? ReadOptions<T>(this ArgsReader args, string name)
		where T : struct, Enum
	{
		var option = args.ReadOption(name);
		if (option is null)
			return null;
		var options = new List<T>();
		do
		{
			options.Add(Enum.TryParse<T>(option, ignoreCase: true, out var value) ? value : throw new ArgsReaderException("Invalid option value"));
			option = args.ReadOption(name);
		}
		while (option is not null);
		return options;
	}
}
