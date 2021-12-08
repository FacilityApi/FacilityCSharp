using System.Diagnostics.CodeAnalysis;

namespace Facility.Core;

/// <summary>
/// Serializes and deserializes values.
/// </summary>
public abstract class ServiceSerializer
{
	/// <summary>
	/// The media type used by default.
	/// </summary>
	public abstract string DefaultMediaType { get; }

	/// <summary>
	/// Serializes a value.
	/// </summary>
	public abstract void ToStream(object? value, Stream stream);

	/// <summary>
	/// Deserializes a value.
	/// </summary>
	public abstract object? FromStream(Stream stream, Type type);

	/// <summary>
	/// Clones a value by serializing and deserializing.
	/// </summary>
	[return: NotNullIfNotNull("value")]
	public virtual T Clone<T>(T value)
	{
		if (value is null)
			return default!;

		using var memoryStream = new MemoryStream();
		ToStream(value, memoryStream);
		memoryStream.Position = 0;
		return (T) FromStream(memoryStream, typeof(T))!;
	}

	public override string ToString() => GetType().Name;
}
