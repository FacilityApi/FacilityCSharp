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
}
